using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace HarvestChoiceApi.Classes
{
    internal class Exceptions
    {
        /// <summary>
        /// creates an <see cref="HttpResponseException"/> with a response code of 400
        /// and places the reason in the reason header and the body.
        /// </summary>
        /// <param name="reason">Explanation text for the client.</param>
        /// <returns>A new HttpResponseException</returns>
        internal static HttpResponseException BadRequest(string reason)
        {
            return CreateHttpResponseException(reason, HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// creates an <see cref="HttpResponseException"/> with a response code of 404
        /// and places the reason in the reason header and the body.
        /// </summary>
        /// <param name="reason">Explanation text for the client.</param>
        /// <returns>A new HttpResponseException</returns>
        internal static HttpResponseException NotFound(string reason)
        {
            return CreateHttpResponseException(reason, HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseException"/> to be thrown by the api.
        /// </summary>
        /// <param name="reason">Explanation text, also added to the body.</param>
        /// <param name="code">The HTTP status code.</param>
        /// <returns>A new <see cref="HttpResponseException"/></returns>
        private static HttpResponseException CreateHttpResponseException(string reason, HttpStatusCode code)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = code,
                ReasonPhrase = reason,
                Content = new StringContent(reason)
            };
            throw new HttpResponseException(response);
        }
    }
}