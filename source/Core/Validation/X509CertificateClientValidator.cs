﻿/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.Owin;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class X509CertificateClientValidator : ClientValidatorBase
    {
        public X509CertificateClientValidator(IClientSecretValidator secretValidator, IClientStore clients)
            : base(secretValidator, clients)
        { }

        protected override async Task<ClientCredential> ExtractCredentialAsync(IDictionary<string, object> environment)
        {
            var credential = new ClientCredential
            {
                IsPresent = false,
                CredentialType = Constants.ClientCredentialTypes.X509Certificate
            };

            var context = new OwinContext(environment);

            // hack to clear a possible cached type from Katana in environment
            context.Environment.Remove("Microsoft.Owin.Form#collection");

            if (!context.Request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await context.Request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                context.Request.Body = copy;
            }

            var body = await context.Request.ReadFormAsync();

            // hack to clear a possible cached type from Katana in environment
            context.Environment.Remove("Microsoft.Owin.Form#collection");
            

            if (body == null)
            {
                return credential;
            }

            var id = body.Get("client_id");
            if (id.IsMissing())
            {
                return credential;
            }

            var cert = context.Get<X509Certificate2>("ssl.ClientCertificate");

            if (cert != null)
            {
                credential.IsPresent = true;
                credential.Credential = cert;

                return credential;
            }

            return credential;
        }
    }
}