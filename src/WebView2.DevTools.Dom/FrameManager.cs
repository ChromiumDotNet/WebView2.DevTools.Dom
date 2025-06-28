using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    internal class FrameManager : IDisposable
    {
        private readonly ConcurrentDictionary<int, ExecutionContext> _contextIdToContext = new ConcurrentDictionary<int, ExecutionContext>();
        private readonly CoreWebView2 _coreWebView2;
        private readonly DevToolsProtocolHelper _devToolsProtocolHelper;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Frame> _frames = new ConcurrentDictionary<string, Frame>();
        private readonly List<string> _isolatedWorlds = new List<string>();
        internal const string UtilityWorldName = "__puppeteer_utility_world__";

        internal FrameManager(CoreWebView2 coreWebView2, DevToolsProtocolHelper devToolsProtocolHelper, ILoggerFactory loggerFactory, WebView2DevToolsContext devToolsContext, TimeoutSettings timeoutSettings)
        {
            _coreWebView2 = coreWebView2;
            _devToolsProtocolHelper = devToolsProtocolHelper;
            DevToolsContext = devToolsContext;
            _logger = loggerFactory.CreateLogger<FrameManager>();
            _loggerFactory = loggerFactory;
            NetworkManager = new NetworkManager(devToolsProtocolHelper);
            TimeoutSettings = timeoutSettings;

            devToolsProtocolHelper.Page.FrameAttached += OnPageFrameAttached;
            devToolsProtocolHelper.Page.FrameNavigated += OnPageFrameNavigated;
            devToolsProtocolHelper.Page.NavigatedWithinDocument += OnPageNavigatedWithinDocument;
            devToolsProtocolHelper.Page.FrameDetached += OnPageFrameDetached;
            devToolsProtocolHelper.Page.FrameStoppedLoading += OnPageFrameStoppedLoading;
            devToolsProtocolHelper.Runtime.ExecutionContextCreated += OnRuntimeExecutionContextCreated;
            devToolsProtocolHelper.Runtime.ExecutionContextDestroyed += OnRuntimeExecutionContextDestroyed;
            devToolsProtocolHelper.Runtime.ExecutionContextsCleared += OnRuntimeExecutionContextsCleared;
            devToolsProtocolHelper.Page.LifecycleEvent += OnPageLifecycleEvent;
        }

        internal event EventHandler<FrameEventArgs> FrameAttached;

        internal event EventHandler<FrameEventArgs> FrameDetached;

        internal event EventHandler<FrameEventArgs> FrameNavigated;

        internal event EventHandler<FrameEventArgs> FrameNavigatedWithinDocument;

        internal event EventHandler<FrameEventArgs> LifecycleEvent;

        internal NetworkManager NetworkManager { get; }

        internal Frame MainFrame { get; set; }

        internal WebView2DevToolsContext DevToolsContext { get; }

        internal TimeoutSettings TimeoutSettings { get; }

        internal ExecutionContext ExecutionContextById(int contextId)
        {
            _contextIdToContext.TryGetValue(contextId, out var context);

            if (context == null)
            {
                _logger.LogError("INTERNAL ERROR: missing context with id = {ContextId}", contextId);
            }
            return context;
        }

        private void OnPageFrameNavigated(object sender, Page.FrameNavigatedEventArgs e)
        {
            OnFrameNavigated(e.Frame);
        }

        private void OnPageFrameStoppedLoading(object sender, Page.FrameStoppedLoadingEventArgs e)
        {
            if (_frames.TryGetValue(e.FrameId, out var frame))
            {
                frame.OnLoadingStopped();
                LifecycleEvent?.Invoke(this, new FrameEventArgs(frame));
            }
        }

        private void OnPageLifecycleEvent(object sender, Page.LifecycleEventEventArgs args)
        {
            if (_frames.TryGetValue(args.FrameId, out var frame))
            {
                frame.OnLifecycleEvent(args.LoaderId, args.Name);
                LifecycleEvent?.Invoke(this, new FrameEventArgs(frame));
            }
        }

        private void OnRuntimeExecutionContextsCleared(object sender, Runtime.ExecutionContextsClearedEventArgs args)
        {
            while (_contextIdToContext.Count > 0)
            {
                var key0 = _contextIdToContext.Keys.ElementAtOrDefault(0);
                if (_contextIdToContext.TryRemove(key0, out var context))
                {
                    if (context.World != null)
                    {
                        context.World.SetContext(null);
                    }
                }
            }
        }

        private void OnRuntimeExecutionContextDestroyed(object sender, Runtime.ExecutionContextDestroyedEventArgs args)
        {
            if (_contextIdToContext.TryRemove(args.ExecutionContextId, out var context))
            {
                if (context.World != null)
                {
                    context.World.SetContext(null);
                }
            }
        }

        private void OnRuntimeExecutionContextCreated(object sender, Runtime.ExecutionContextCreatedEventArgs args)
        {
            var contextPayload = args.Context;

            var auxData = (System.Text.Json.JsonElement)contextPayload.AuxData;
            var frameId = auxData.GetProperty("frameId").GetString();
            var isDefault = auxData.GetProperty("isDefault").GetBoolean();

            Frame frame = null;
            DOMWorld world = null;

            if (!string.IsNullOrEmpty(frameId))
            {
                frame = GetFrames().FirstOrDefault(x => x.Id == frameId);
            }

            if (frame != null)
            {
                if (isDefault)
                {
                    world = frame.MainWorld;
                }
                else if (contextPayload.Name == UtilityWorldName && !frame.SecondaryWorld.HasContext)
                {
                    // In case of multiple sessions to the same target, there's a race between
                    // connections so we might end up creating multiple isolated worlds.
                    // We can use either.
                    world = frame.SecondaryWorld;
                }
            }

            var context = new ExecutionContext(_coreWebView2, _devToolsProtocolHelper, _loggerFactory, contextPayload.Id, world);
            if (world != null)
            {
                world.SetContext(context);
            }
            _contextIdToContext[contextPayload.Id] = context;
        }

        private void OnPageFrameDetached(object sender, Page.FrameDetachedEventArgs e)
        {
            if (e.Reason == "remove")
            {
                // Only remove the frame if the reason for the detached event is
                // an actual removement of the frame.
                // For frames that become OOP iframes, the reason would be 'swap'.
                if (_frames.TryGetValue(e.FrameId, out var frame))
                {
                    RemoveFramesRecursively(frame);
                }
            }
        }

        private void OnFrameNavigated(Page.Frame f)
        {
            var isMainFrame = string.IsNullOrEmpty(f.ParentId);
            Frame frame;

            if (isMainFrame)
            {
                frame = MainFrame;
            }
            else
            {
                if (!_frames.TryGetValue(f.Id, out frame))
                {
                    throw new WebView2DevToolsContextException("TODO: Unable to locate frame");
                }
            }
            Contract.Assert(isMainFrame || frame != null, "We either navigate top level or have old version of the navigated frame");

            // Detach all child frames first.
            if (frame != null)
            {
                while (frame.ChildFrames.Count > 0)
                {
                    RemoveFramesRecursively(frame.ChildFrames[0]);
                }
            }

            // Update or create main frame.
            if (isMainFrame)
            {
                if (frame != null)
                {
                    // Update frame id to retain frame identity on cross-process navigation.
                    if (frame.Id != null)
                    {
                        _frames.TryRemove(frame.Id, out _);
                    }
                    frame.Id = f.Id;
                }
                else
                {
                    // Initial main frame navigation.
                    frame = new Frame(this, _devToolsProtocolHelper, null, f.Id, isMainFrame);
                }
                _frames.TryAdd(f.Id, frame);
                MainFrame = frame;
            }

            frame.Name = f.Name ?? string.Empty;
            frame.Url = f.Url + f.UrlFragment;

            FrameNavigated?.Invoke(this, new FrameEventArgs(frame));
        }

        internal Frame[] GetFrames() => _frames.Values.ToArray();

        private void OnPageNavigatedWithinDocument(object sender, Page.NavigatedWithinDocumentEventArgs e)
        {
            if (_frames.TryGetValue(e.FrameId, out var frame))
            {
                frame.NavigatedWithinDocument(e.Url);

                var eventArgs = new FrameEventArgs(frame);
                FrameNavigatedWithinDocument?.Invoke(this, eventArgs);
                FrameNavigated?.Invoke(this, eventArgs);
            }
        }

        private void RemoveFramesRecursively(Frame frame)
        {
            while (frame.ChildFrames.Count > 0)
            {
                RemoveFramesRecursively(frame.ChildFrames[0]);
            }
            frame.Detach();
            _frames.TryRemove(frame.Id, out _);
            FrameDetached?.Invoke(this, new FrameEventArgs(frame));
        }

        private void OnPageFrameAttached(object sender, Page.FrameAttachedEventArgs frameAttached)
            => OnFrameAttached(frameAttached.FrameId, frameAttached.ParentFrameId);

        private void OnFrameAttached(string frameId, string parentFrameId)
        {
            if (!_frames.ContainsKey(frameId) && _frames.ContainsKey(parentFrameId))
            {
                var isMainFrame = string.IsNullOrEmpty(parentFrameId);
                var parentFrame = _frames[parentFrameId];
                var frame = new Frame(this, _devToolsProtocolHelper, parentFrame, frameId, isMainFrame);
                _frames[frame.Id] = frame;
                FrameAttached?.Invoke(this, new FrameEventArgs(frame));
            }
        }

        internal void LoadFrameTree(FrameTree frameTree)
        {
            if (!string.IsNullOrEmpty(frameTree.Frame.ParentId))
            {
                OnFrameAttached(frameTree.Frame.Id, frameTree.Frame.ParentId);
            }

            OnFrameNavigated(frameTree.Frame);

            if (frameTree.Childs != null)
            {
                foreach (var child in frameTree.Childs)
                {
                    LoadFrameTree(child);
                }
            }
        }

        internal async Task EnsureIsolatedWorldAsync(string name)
        {
            if (_isolatedWorlds.Contains(name))
            {
                return;
            }
            _isolatedWorlds.Add(name);
            await _devToolsProtocolHelper.Page.AddScriptToEvaluateOnNewDocumentAsync(
                source: $"//# sourceURL={ExecutionContext.EvaluationScriptUrl}",
                worldName: name).ConfigureAwait(true);

            await Task.WhenAll(GetFrames().Select(frame => _devToolsProtocolHelper.Page.CreateIsolatedWorldAsync(
                    frameId: frame.Id,
                    grantUniveralAccess: true,
                    worldName: name))).ConfigureAwait(true);
        }

        public void Dispose()
        {
            FrameAttached = null;
            FrameDetached = null;
            FrameNavigated = null;
            FrameNavigatedWithinDocument = null;
            LifecycleEvent = null;

            _devToolsProtocolHelper.Page.FrameAttached -= OnPageFrameAttached;
            _devToolsProtocolHelper.Page.FrameNavigated -= OnPageFrameNavigated;
            _devToolsProtocolHelper.Page.NavigatedWithinDocument -= OnPageNavigatedWithinDocument;
            _devToolsProtocolHelper.Page.FrameDetached -= OnPageFrameDetached;
            _devToolsProtocolHelper.Page.FrameStoppedLoading -= OnPageFrameStoppedLoading;
            _devToolsProtocolHelper.Runtime.ExecutionContextCreated -= OnRuntimeExecutionContextCreated;
            _devToolsProtocolHelper.Runtime.ExecutionContextDestroyed -= OnRuntimeExecutionContextDestroyed;
            _devToolsProtocolHelper.Runtime.ExecutionContextsCleared -= OnRuntimeExecutionContextsCleared;
            _devToolsProtocolHelper.Page.LifecycleEvent -= OnPageLifecycleEvent;
        }
    }
}
