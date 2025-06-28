// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/aspnetcore/blob/a450cb69b5e4549f5515cdb057a68771f56cefd7/src/Http/WebUtilities/src/ReasonPhrases.cs#L90

using System.Collections.Generic;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Provides access to HTTP status code reason phrases as listed in
    /// http://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml.
    /// </summary>
    internal static class ReasonPhrases
    {
        // Status Codes listed at http://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
        private static readonly Dictionary<int, string> Phrases = new Dictionary<int, string>()
        {
            { 100, "Continue" },
            { 101, "Switching Protocols" },
            { 102, "Processing" },
            { 200, "OK" },
            { 201, "Created" },
            { 202, "Accepted" },
            { 203, "Non-Authoritative Information" },
            { 204, "No Content" },
            { 205, "Reset Content" },
            { 206, "Partial Content" },
            { 207, "Multi-Status" },
            { 208, "Already Reported" },
            { 226, "IM Used" },
            { 300, "Multiple Choices" },
            { 301, "Moved Permanently" },
            { 302, "Found" },
            { 303, "See Other" },
            { 304, "Not Modified" },
            { 305, "Use Proxy" },
            { 306, "Switch Proxy" },
            { 307, "Temporary Redirect" },
            { 308, "Permanent Redirect" },
            { 400, "Bad Request" },
            { 401, "Unauthorized" },
            { 402, "Payment Required" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 405, "Method Not Allowed" },
            { 406, "Not Acceptable" },
            { 407, "Proxy Authentication Required" },
            { 408, "Request Timeout" },
            { 409, "Conflict" },
            { 410, "Gone" },
            { 411, "Length Required" },
            { 412, "Precondition Failed" },
            { 413, "Payload Too Large" },
            { 414, "URI Too Long" },
            { 415, "Unsupported Media Type" },
            { 416, "Range Not Satisfiable" },
            { 417, "Expectation Failed" },
            { 418, "I'm a teapot" },
            { 419, "Authentication Timeout" },
            { 421, "Misdirected Request" },
            { 422, "Unprocessable Entity" },
            { 423, "Locked" },
            { 424, "Failed Dependency" },
            { 426, "Upgrade Required" },
            { 428, "Precondition Required" },
            { 429, "Too Many Requests" },
            { 431, "Request Header Fields Too Large" },
            { 451, "Unavailable For Legal Reasons" },
            { 500, "Internal Server Error" },
            { 501, "Not Implemented" },
            { 502, "Bad Gateway" },
            { 503, "Service Unavailable" },
            { 504, "Gateway Timeout" },
            { 505, "HTTP Version Not Supported" },
            { 506, "Variant Also Negotiates" },
            { 507, "Insufficient Storage" },
            { 508, "Loop Detected" },
            { 510, "Not Extended" },
            { 511, "Network Authentication Required" },
        };

        /// <summary>
        /// Gets the reason phrase for the specified status code.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>The reason phrase, or <see cref="string.Empty"/> if the status code is unknown.</returns>
        public static string GetReasonPhrase(int statusCode)
        {
            return Phrases.TryGetValue(statusCode, out var phrase) ? phrase : string.Empty;
        }
    }
}
