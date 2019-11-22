using System;
using System.Collections.Generic;
using System.Xml;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines.Upload;

namespace UploadFileContentTypeBlocker
{
    /// <summary>
    /// Cancel file uploading if content type is marked as disabled to be uploaded
    /// </summary>
    public class FilterBlockedContentType : UploadProcessor
    {

        private readonly List<string> _filteredContentTypes = new List<string>();

        /// <summary>
        /// Add the blacklisted content types
        /// </summary>
        /// <param name="configNode"></param>
        protected virtual void AddRestrictedContentType(XmlNode configNode)
        {
            if (configNode == null)
            {
                return;
            }

            var contentTypes = configNode.InnerText.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var types in contentTypes)
                _filteredContentTypes.Add(types);
        }

        private bool IsFilteredContentType(string contentType)
        {
            return _filteredContentTypes.Exists(type =>
                string.Equals(type, contentType, StringComparison.CurrentCultureIgnoreCase));

        }

        /// <summary>
        /// Runs the processor
        /// </summary>
        /// <param name="args"></param>
        public void Process(UploadArgs args)
        {
            foreach (string fileKey in args.Files)
            {
                var fileName = args.Files[fileKey]?.FileName;
                var contentType = args.Files[fileKey]?.ContentType;

                if (IsFilteredContentType(contentType))
                {
                    var file = StringUtil.EscapeJavascriptString(fileName);
                    var reason = StringUtil.EscapeJavascriptString("File upload is restricted.");

                    args.UiResponseHandlerEx.FileCannotBeUploaded(file, reason);

                    args.ErrorText = Translate.Text($"The file \"{fileName}\" cannot be uploaded");
                    Log.Error(args.ErrorText, this);
                    args.AbortPipeline();
                    break;
                }


            }
        }

    }
}