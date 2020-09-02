using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using HttpRestDriveAccess.Models;
using GData = Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using HttpRestDriveAccess.Helpers;
using System.IO;
using Google.Apis.Download;
using Google.Apis.Util;
using Newtonsoft.Json;
using Google.Apis.Upload;

namespace HttpRestDriveAccess.Services
{
    public interface IHomeServices
    {
        List<GoogleDriveFiles> LoadFiles(string accessToken);
        void UploadFile(IFormFile file, string accessToken);
        void DownloadFile(string fileId, string accessToken);
    }

    public class HomeServices : IHomeServices
    {
        public void DownloadFile(string fileId, string accessToken)
        {
            try
            {
                var service = GetDriveServiceByAccessToken(accessToken);

                var request = service.Files.Get(fileId);

                var stream = new MemoryStream();

                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                Debug.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                string downloadFolder = $"{Path.GetPathRoot(Environment.SystemDirectory)}Google Drive Downloads";
                                SaveStream(stream, downloadFolder, request.Execute().Name);

                                string serverPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloades");
                                SaveStream(stream, serverPath, request.Execute().Name);

                                stream.Close();

                                Debug.WriteLine($"Download complete at {downloadFolder}");

                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                Debug.WriteLine("Download failed.");
                                break;
                            }
                    }
                };

                request.Download(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException);
            }
        }

        public List<GoogleDriveFiles> LoadFiles(string accessToken)
        {
            try
            {
                var service = GetDriveServiceByAccessToken(accessToken);

                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 100;
                listRequest.Fields = "nextPageToken, files(id, name, size)";

                IList<GData.File> files = listRequest.Execute().Files;
                List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        GoogleDriveFiles File = new GoogleDriveFiles
                        {
                            Id = file.Id,
                            Name = file.Name,
                            Size = file.Size
                        };

                        FileList.Add(File);
                    }
                }

                return FileList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException);
                return new List<GoogleDriveFiles>();
            }
        }

        public void UploadFile(IFormFile file, string accessToken)
        {
            try
            {
                FilesResource.CreateMediaUpload request;
                var service = GetDriveServiceByAccessToken(accessToken);

                GData.File fileMetadata = new GData.File()
                {
                    Id = null,
                    Name = file.FileName,
                    MimeType = file.ContentType,
                    Description = "File uploaded from OAuth Client Drive Access."
                };

                using (Stream stream = file.OpenReadStream())
                {
                    request = service.Files.Create(fileMetadata, stream, file.ContentType);
                    request.Fields = "id";

                    request.ProgressChanged += ProgressChanged;
                    request.ResponseReceived += ResponseReceived;

                    var progress = request.Upload();

                    if (progress.Exception != null)
                    {
                        Logs.LogHandler($"progress.Exception: {progress.Exception.Message.ToString()}");
                    }

                    Logs.LogHandler($"uploaded: {request?.ResponseBody?.Id ?? null}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Logs.LogHandler($"Upload Error: {ex.ToString()}");
            }
        }

        private void ResponseReceived(GData.File obj)
        {
            Debug.WriteLine($"File uploaded successfully  {obj.Id}");
        }

        private void ProgressChanged(IUploadProgress uploadProgress)
        {
            Debug.WriteLine($"{uploadProgress.Status} {uploadProgress.BytesSent}");
            Logs.LogHandler($"Upload Progress: {uploadProgress.Status} {uploadProgress.BytesSent}");
        }

        protected DriveService GetDriveServiceByAccessToken(string _accessToken)
        {
            GoogleCredential googleCredential = GoogleCredential.FromAccessToken(_accessToken);

            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = "OAuth client"
            });

            return service;
        }

        protected static void SaveStream(MemoryStream stream, string saveTo, string fileName)
        {
            try
            {
                if (!Directory.Exists(saveTo)) Directory.CreateDirectory(saveTo);

                FileStream fileStream = new FileStream(Path.Combine(saveTo, fileName), FileMode.Create, FileAccess.Write);
                stream.WriteTo(fileStream);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException);
                Logs.LogHandler($"File saving Error: {ex.ToString()}");
            }
        }

        public static string SaveFile(string path, IFormFile file)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, file.FileName);

            Stream stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);
            stream.Close();

            return filePath;
        }
    }
}
