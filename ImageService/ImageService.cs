using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.IO;
using System.Collections;
using System.Configuration;

using ImageService;
using ImageService.Contracts;

namespace ImageServicing
{
    [ServiceBehavior]
    public class ImageService : IImageService
    {
        public bool Status(IEnumerable<string> clientImagesList)
        {
            Log("Status");
            try
            {
                if (GetImagesCount() == clientImagesList.Count())
                    return true;
                else
                    return false;
            }
            catch (FaultException e)
            { throw e; }

        }
 
        public IEnumerable<ImageFileData>  GetAllImagesList(bool withFilesData)
        {
            Log("GetAllImagesList");
            List<ImageFileData> images_collection = new List<ImageFileData>();
            try
            {
                IEnumerable<FileInfo> allImageFiles = GetAllImageFiles();
                if (allImageFiles == null)
                    return null;

                foreach (FileInfo fileInfo in allImageFiles)
                {
                    ImageFileData imageFileData = new ImageFileData() { FileName = fileInfo.Name, LastDateModified = fileInfo.LastWriteTime };
                    byte[] imageBytes = null;
                    if (withFilesData)
                        imageBytes = File.ReadAllBytes(fileInfo.FullName);
                    imageFileData.ImageData = imageBytes;
                    images_collection.Add(imageFileData);
                }
            }
            catch (Exception)
            {
                HostStorageException fault = new HostStorageException();
                fault.Description = "Prodlems with images storage on host";
                throw new FaultException<HostStorageException>(fault);
            }
            return images_collection;
        }

        public ImageFileData GetImageByName(string request_file_name)
        {
            Log("GetImageByName");
            try
            {
                if (string.IsNullOrEmpty(request_file_name))
                    throw new FaultException<InvalidFileName>(new InvalidFileName { InvalidName = request_file_name });

                IEnumerable<FileInfo> allImageFilesList = GetAllImageFiles();
                FileInfo requestedImageFile = null;
                requestedImageFile = allImageFilesList.SingleOrDefault(f => f.Name == request_file_name);
                if (requestedImageFile == null)
                    throw new FaultException<FileNotFound>(new FileNotFound { FileName = request_file_name });

                ImageFileData imageFileData = new ImageFileData() { FileName = requestedImageFile.Name, LastDateModified = requestedImageFile.LastWriteTime };
                byte[] imageBytes = File.ReadAllBytes(requestedImageFile.FullName);
                imageFileData.ImageData = imageBytes;
                return imageFileData;
            }

            catch (FaultException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                HostStorageException fault = new HostStorageException();
                fault.Description = "Prodlems with images storage on host";
                throw new FaultException<HostStorageException>(fault);
            }
        }

        public void UploadImage(ImageFileData uploading_image)
        {
            Log("UploadImage");
            try
            {
                if (string.IsNullOrEmpty(uploading_image.FileName))
                    throw new ArgumentException("Invalid upldoading file name", uploading_image.FileName);

                if (uploading_image.ImageData == null || uploading_image.ImageData.Length == 0)
                    throw new ArgumentException("Uploaded file-data is empty!");

                string newImageFileName = uploading_image.FileName;
                string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
                string newImageFilePath = Path.Combine(uploadFolder, newImageFileName);

                if (!File.Exists(newImageFilePath))
                    throw new FaultException<FileAlreadyExists>(new FileAlreadyExists {FileName = newImageFileName});

                using (Stream targetStream = new FileStream(newImageFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    targetStream.Write(uploading_image.ImageData, 0, uploading_image.ImageData.Length);
                }
            }
            
            catch (FaultException e)
            {
                throw e;
            }
            catch (Exception)
            {
                HostStorageException fault = new HostStorageException();
                fault.Description = "Prodlems with images storage on host";
                throw new FaultException<HostStorageException>(fault, new FaultReason(fault.Description));
            }
        }

        private IEnumerable<FileInfo> GetAllImageFiles()
        {
            string   uploadFolder    = ConfigurationManager.AppSettings["UploadFolder"];
            string[] imageExtentions = ConfigurationManager.AppSettings["ImageSearchPattern"].Split(' ');

            IEnumerable<FileInfo> imageFilesList = null;
            try
            {
                if(!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);
                DirectoryInfo dir = new DirectoryInfo(uploadFolder);
                imageFilesList = (from file in dir.GetFiles("*.*", SearchOption.AllDirectories)
                                 where imageExtentions.Contains(file.Extension)
                                 select file).ToArray();
            }
            catch (Exception e)
            {
                throw e;
            }

            return imageFilesList;
        }

        private int GetImagesCount()
        { 
            int imagesCount = 0;
            try
            {
                imagesCount = (new List<FileInfo>(GetAllImageFiles())).Count;
            }
            catch (Exception e)
            {
                throw e;
            }
            return imagesCount;
        }

        private void Log(string methodName)
        {
            Console.WriteLine("{0} is calling by {1}", methodName, OperationContext.Current.SessionId);
            Console.WriteLine();
        }
    }
}
