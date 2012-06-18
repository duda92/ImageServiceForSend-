using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace ImageService.Contracts
{
    [ServiceContract]
    public interface IImageService
    {
        /// <summary>
        /// Gets all images list existing on the server
        /// </summary>
        /// <param name="withData">
        /// define does it required to send filenames with byte streams
        /// </param>
        /// <returns>
        /// IEnumerable instanse which contains instanses of DataContract
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(HostStorageException))]
        IEnumerable<ImageFileData> GetAllImagesList(bool withData);

        /// <summary>
        /// Gets the Image-file stream by image-name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// ImageFileData-object which contains filename, last date modified and bytes array of image-file
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(HostStorageException))]
        [FaultContract(typeof(InvalidFileName))]
        [FaultContract(typeof(FileNotFound))]
        ImageFileData GetImageByName(string fileName);

        /// <summary>
        /// Uploads the image file stream to server
        /// </summary>
        /// <param name="uploading_image">The ImageFileData-object which contains filename, last date modified (not necessarily) and bytes array of image-file for upload.</param>
        [OperationContract]
        [FaultContract(typeof(HostStorageException))]
        [FaultContract(typeof(FileAlreadyExists))]
        void UploadImage(ImageFileData uploading_image);

        /// <summary>
        /// Returns the status of client by session
        /// </summary>
        /// <returns>true if status is actual, otherwise false</returns>
        [OperationContract]
        [FaultContract(typeof(HostStorageException))]
        bool Status(IEnumerable<string> clientImagesList);

    }

    #region MessageContract
    /// <summary>
    /// Data type for exchange clent-server by images
    /// </summary>
    [DataContract()]
    public class ImageFileData
    {
        /// <summary>
        /// This field contains name of image file uploaded or downloaded 
        /// </summary>
        [DataMember]
        public string FileName {get; set;}

        /// <summary>
        /// The date time of last modifying image
        /// Use for response to client of service on its request of getting all images list 
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        [DataMember]
        public DateTime LastDateModified { get; set; }

        /// <summary>
        /// Bytes of image file stream
        /// Used when cleint upload image or when response to client on its request of image by name and (depends from parameter) GetAllImages
        /// </summary>
        [DataMember]
        public byte[] ImageData {get; set;}
    }
    #endregion

    #region FaultContract
    ///// <summary>
    ///// Data-type server fault (fault contract)
    ///// </summary>
    //[DataContract]
    //public class ServerFault
    //{
    //    /// <summary>
    //    /// Contains the friendly discription of just occured fault on server 
    //    /// </summary>
    //    /// <value>
    //    /// The friendly discription.
    //    /// </value>
    //    [DataMember]
    //    public string FriendlyDiscription { get; set; }

    //    /// <summary>
    //    /// Contains message of exception just occured on server.
    //    /// </summary>
    //    /// <value>
    //    /// The exception message.
    //    /// </value>
    //    [DataMember]
    //    public string Discription { get; set; }
    //}

    [DataContract]
    public class FileNotFound
    {
        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract]
    public class FileAlreadyExists
    {
        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract]
    public class HostStorageException
    {
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class InvalidFileName
    {
        [DataMember]
        public string InvalidName { get; set; }
    }

    #endregion
}
