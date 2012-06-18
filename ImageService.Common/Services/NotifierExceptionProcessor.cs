using System;
using System.ServiceModel;
using ImageService;
using ImageService.Contracts;

namespace ImageService.Common
{
    public class NotifierExceptionProcessor : IFaultExceptionProcessor
    {
        public NotifierExceptionProcessor(IClientNotifier notifier)
        {
            Notifier = notifier;
        }

        public IClientNotifier Notifier { get; private set;  }

        public void ProcessException(Exception ex)
        {
            if (Notifier != null)
            {
                if (ex is EndpointNotFoundException)
                    Notifier.Error("Endpoint not found!");
                if (ex is CommunicationObjectFaultedException || ex is System.ServiceModel.CommunicationException)
                    Notifier.Error("Service host is unavailable now");
            
                //---------------
                if (ex is FaultException<FileNotFound>)
                    Notifier.Error("File " + (ex as FaultException<FileNotFound>).Detail.FileName + " not found on host");
                if (ex is FaultException<FileAlreadyExists>)
                    Notifier.Warning("File " + (ex as FaultException<FileAlreadyExists>).Detail.FileName + " already exists on host");
                if (ex is FaultException<HostStorageException>)
                    Notifier.Message((ex as FaultException<HostStorageException>).Detail.Description);
                if (ex is FaultException<InvalidFileName>)
                    Notifier.Error("File name in request \"" + (ex as FaultException<InvalidFileName>).Detail.InvalidName + "\"is invalid");

            }
            else
                throw ex;
        }

        public void ProcessMessage(string message)
        {
            if (Notifier != null)
            {
                Notifier.Message(message);
            }
            else
                Console.WriteLine("Message form manager {0}: ",message);
        }
    }
}
