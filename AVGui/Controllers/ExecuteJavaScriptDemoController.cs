// -----------------------------------------------------------------------
// <copyright file="ExecuteJavaScriptDemoController.cs" company="Weloveloli">
//     Copyright (c) Weloveloli.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Weloveloli.AVGui.Controllers
{
    using System;
    using System.Text.Json;
    using Chromely.Core.Configuration;
    using Chromely.Core.Logging;
    using Chromely.Core.Network;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The demo controller.
    /// </summary>
    [ControllerProperty(Name = "ExecuteJavaScriptDemoController")]
    public class ExecuteJavaScriptDemoController : ChromelyController
    {
        /// <summary>
        /// Defines the _config.
        /// </summary>
        private readonly IChromelyConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteJavaScriptDemoController"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="IChromelyConfiguration"/>.</param>
        public ExecuteJavaScriptDemoController(IChromelyConfiguration config)
        {
            _config = config;
            RegisterRequest("/executejavascript/execute", Execute);
        }

        /// <summary>
        /// The Execute.
        /// </summary>
        /// <param name="request">The request<see cref="IChromelyRequest"/>.</param>
        /// <returns>The <see cref="IChromelyResponse"/>.</returns>
        private IChromelyResponse Execute(IChromelyRequest request)
        {
            var response = new ChromelyResponse(request.Id)
            {
                ReadyState = (int)ReadyState.ResponseIsReady,
                Status = (int)System.Net.HttpStatusCode.OK,
                StatusText = "OK",
                Data = DateTime.Now.ToLongDateString()
            };

            try
            {
                var scriptInfo = new ScriptInfo(request.PostData);
                var scriptExecutor = _config?.JavaScriptExecutor;
                if (scriptExecutor == null)
                {
                    response.Data = $"Frame {scriptInfo.FrameName} does not exist.";
                    return response;
                }

                scriptExecutor.ExecuteScript(scriptInfo.Script);
                response.Data = "Executed script :" + scriptInfo.Script;
                return response;
            }
            catch (Exception e)
            {
                Logger.Instance.Log.LogError(e, e.Message);
                response.Data = e.Message;
                response.ReadyState = (int)ReadyState.RequestReceived;
                response.Status = (int)System.Net.HttpStatusCode.BadRequest;
                response.StatusText = "Error";
            }

            return response;
        }

        /// <summary>
        /// Defines the <see cref="ScriptInfo" />.
        /// </summary>
        private class ScriptInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ScriptInfo"/> class.
            /// </summary>
            /// <param name="postData">The postData<see cref="object"/>.</param>
            public ScriptInfo(object postData)
            {
                FrameName = string.Empty;
                Script = string.Empty;
                if (postData != null)
                {
                    var options = new JsonSerializerOptions();
                    options.ReadCommentHandling = JsonCommentHandling.Skip;
                    options.AllowTrailingCommas = true;
                    var requestData = JsonSerializer.Deserialize<scriptInfo>(postData.ToString(), options);

                    FrameName = requestData.framename ?? string.Empty;
                    Script = requestData.script ?? string.Empty;
                }
            }

            /// <summary>
            /// Gets the frame name...
            /// </summary>
            public string FrameName { get; }

            /// <summary>
            /// Gets the Script.
            /// </summary>
            public string Script { get; }
        }

        /// <summary>
        /// Defines the <see cref="scriptInfo" />.
        /// </summary>
        private class scriptInfo
        {
            /// <summary>
            /// Gets or sets the framename.
            /// </summary>
            public string framename { get; set; }

            /// <summary>
            /// Gets or sets the script.
            /// </summary>
            public string script { get; set; }
        }
    }
}
