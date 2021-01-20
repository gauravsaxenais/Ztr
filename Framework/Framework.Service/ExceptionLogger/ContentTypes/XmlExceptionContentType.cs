namespace ZTR.Framework.Service.ExceptionLogger.ContentTypes
{
    using System.IO;
    using System.Xml.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using Service;

    /// <summary>
    /// XmlExceptionContentType
    /// </summary>
    /// <seealso cref="ZTR.Framework.Service.ExceptionLogger.ContentTypes.AbstractExceptionContentType" />
    public class XmlExceptionContentType : AbstractExceptionContentType
    {
        /// <summary>
        /// Creates the exception response.
        /// </summary>
        /// <param name="problemDetails">The problem details.</param>
        /// <returns></returns>
        public override ExceptionResponse CreateExceptionResponse(ProblemDetails problemDetails)
        {
            string stringWriter;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProblemDetails));
            using (StringWriter writer = new StringWriter())
            {
                xmlSerializer.Serialize(writer, problemDetails);
                stringWriter = writer.ToString();
            }

            return new ExceptionResponse(SupportedContentTypes.ProblemDetailsXml, stringWriter);
        }
    }
}
