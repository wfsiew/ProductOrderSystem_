using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using Google.GData.AccessControl;
using Google.GData.Apps;
using Google.GData.Client;
using Google.GData.Documents;
using GoogleAppHelper.Constants;
using GoogleAppHelper.Models;

namespace GoogleAppHelper.Services
{
    public class DocumentService
    {
        private string GoogleAppDomain;
        private string GoogleConsumerKey;
        private string GoogleConsumerSecret;
        private string GoogleApplicationName;
        private bool UseAdminLogin;
        private GOAuthRequestFactory requestFactory;

        public DocumentService()
            : this(ConfigurationManager.AppSettings["GoogleApplicationName"],
                ConfigurationManager.AppSettings["GoogleAppDomain"],
                ConfigurationManager.AppSettings["GoogleConsumerKey"],
                ConfigurationManager.AppSettings["GoogleConsumerSecret"])
        {
        }

        public DocumentService(string googleApplicationName, string googleAppDomain, string googleConsumerKey, string googleConsumerSecret)
        {
            GoogleAppDomain = googleAppDomain;
            GoogleConsumerKey = googleConsumerKey;
            GoogleConsumerSecret = googleConsumerSecret;
            GoogleApplicationName = googleApplicationName;
            UseAdminLogin = Convert.ToBoolean(ConfigurationManager.AppSettings["GoogleUseAdminLogin"]);
            requestFactory = new GOAuthRequestFactory("writely", GoogleApplicationName);
            requestFactory.ConsumerKey = ConfigurationManager.AppSettings["GoogleConsumerKey"];
            requestFactory.ConsumerSecret = ConfigurationManager.AppSettings["GoogleConsumerSecret"];
        }

        public void DeleteDocument(string resourceID, string requestorId)
        {
            DocumentsService service = new DocumentsService(GoogleApplicationName);
            string username = requestorId.Split('@')[0];

            try
            {
                if (UseAdminLogin)
                {
                    service.setUserCredentials(Constants.Constants.ADMIN_EMAIL, Constants.Constants.ADMIN_PWD);
                }

                else
                {
                    service.RequestFactory = requestFactory;
                }

                Uri postUri = new Uri(string.Format(DocumentsListQuery.documentsBaseUri + "/{0}?xoauth_requestor_id={1}&delete=true", resourceID, requestorId));

                service.Delete(postUri, "*");
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        //role reader, writer
        public void ShareDocumentToEmail(DocumentEntry entry, string shareToEmail, DocumentShareRole role, bool sendEmail)
        {
            try
            {
                ShareDocument(entry, shareToEmail, "user", role.ToString(), sendEmail);
            }

            catch (Exception)
            {
                //dun do anything if fail
            }
        }

        public void ShareDocumentToDomain(DocumentEntry entry, DocumentShareRole role, bool sendEmail)
        {
            try
            {
                ShareDocument(entry, GoogleAppDomain, "domain", role.ToString(), sendEmail);
            }

            catch (Exception)
            {
                //dun do anything if fail
            }
        }

        private void ShareDocument(DocumentEntry entry, string shareToName, string shareToType, string role, bool sendEmail)
        {
            try
            {
                DocumentsService service = new DocumentsService(GoogleApplicationName);
                if (UseAdminLogin)
                {
                    service.setUserCredentials(Constants.Constants.ADMIN_EMAIL, Constants.Constants.ADMIN_PWD);
                }
                else
                {
                    service.RequestFactory = requestFactory;
                }

                AclEntry acl = new AclEntry();
                acl.Scope = new AclScope(shareToName, shareToType);
                acl.Role = new AclRole(role);
                Uri uri = new Uri(string.Format(entry.AccessControlList + "&send-notification-emails={0}", (sendEmail ? "true" : "false")));
                service.Insert(uri, acl);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GoogleDocument UploadFile2LO(Stream fileStream, string fileName, string contentType, bool convert, string requestorId, string[] ShareEmails)
        {
            DocumentEntry entry = null;

            //FileInfo fileInfo = new FileInfo(fileName);
            //FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            string username = requestorId.Split('@')[0];

            try
            {
                DocumentsService service = new DocumentsService(GoogleApplicationName);

                if (UseAdminLogin)
                {
                    service.setUserCredentials(Constants.Constants.ADMIN_EMAIL, Constants.Constants.ADMIN_PWD);
                }

                else
                {
                    service.RequestFactory = requestFactory;
                }

                Uri postUri;

                if (!convert)
                {
                    postUri = new OAuthUri(DocumentsListQuery.documentsBaseUri + "?convert=false", username, GoogleAppDomain);
                }
                else
                {
                    postUri = new OAuthUri(DocumentsListQuery.documentsBaseUri, username, GoogleAppDomain);
                }

                if (contentType == null)
                {
                    throw new ArgumentException("You need to specify a content type, like text/html");
                }

                entry = service.Insert(postUri, fileStream, contentType, fileName) as DocumentEntry;

                //foreach (String email in ShareEmails)
                //{
                //    this.ShareDocumentToEmail(entry, email, DocumentShareRole.writer, false);
                //}
                ShareDocumentToDomain(entry, DocumentShareRole.writer, false);
                //todo: local test
                //ShareDocumentToEmail(entry, "siewwingfei@hotmail.com", DocumentShareRole.writer, true);

                GoogleDocument document = new GoogleDocument()
                {
                    Title = entry.Title.Text,
                    ResourceID = entry.ResourceId.ToString(),
                    AlternateUri = entry.AlternateUri.ToString(),
                    Updated = entry.Updated,
                    DownloadUrl = entry.Content.Src.Content,
                    OriginalEntry = entry
                };

                return document;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DocumentEntry UploadFile2LO(string fileName, string documentName, string contentType, bool convert, string requestorId)
        {
            DocumentEntry entry = null;
            FileInfo fileInfo = new FileInfo(fileName);
            FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            string username = requestorId.Split('@')[0];

            try
            {
                DocumentsService service = new DocumentsService(GoogleApplicationName);
                if (UseAdminLogin)
                {
                    service.setUserCredentials(Constants.Constants.ADMIN_EMAIL, Constants.Constants.ADMIN_PWD);
                }

                else
                {
                    service.RequestFactory = requestFactory;
                }

                Uri postUri;

                if (!convert)
                {
                    postUri = new OAuthUri(DocumentsListQuery.documentsBaseUri + "?convert=false", username, GoogleAppDomain);
                }

                else
                {
                    postUri = new OAuthUri(DocumentsListQuery.documentsBaseUri, username, GoogleAppDomain);
                }

                if (documentName == null)
                {
                    documentName = fileInfo.Name;
                }

                if (contentType == null)
                {
                    throw new ArgumentException("You need to specify a content type, like text/html");
                }

                entry = service.Insert(postUri, stream, contentType, documentName) as DocumentEntry;
            }

            finally
            {
                stream.Close();
            }

            return entry;
        }

        public List<GoogleDocument> GetDocumentsByRequestorID(string requestorId)
        {

            List<GoogleDocument> googledocuments = new List<GoogleDocument>();
            try
            {
                DocumentsService service = new DocumentsService(GoogleApplicationName);

                if (UseAdminLogin)
                {
                    service.setUserCredentials(Constants.Constants.ADMIN_EMAIL, Constants.Constants.ADMIN_PWD);
                }

                else
                {
                    service.RequestFactory = requestFactory;
                }

                // Retrieve user's list of Google Docs
                DocumentsListQuery query = new DocumentsListQuery();
                query.Uri = new OAuthUri("https://docs.google.com/feeds/default/private/full", "USER", "Domain");
                query.OAuthRequestorId = requestorId;

                DocumentsFeed feed = service.Query(query);

                foreach (DocumentEntry entry in feed.Entries)
                {
                    googledocuments.Add(new GoogleDocument()
                    {
                        Title = entry.Title.Text,
                        ResourceID = entry.ResourceId.ToString(),
                        AlternateUri = entry.AlternateUri.ToString(),
                        Updated = entry.Updated,
                        DownloadUrl = entry.Content.Src.Content,
                        OriginalEntry = entry
                    });
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }

            return googledocuments;
        }
    }
}
