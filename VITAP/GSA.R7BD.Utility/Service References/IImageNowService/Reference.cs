﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GSA.R7BD.Utility.IImageNowService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="IImageNowService.IImageNowService")]
    public interface IImageNowService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IImageNowService/SayHello", ReplyAction="http://tempuri.org/IImageNowService/SayHelloResponse")]
        string SayHello(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IImageNowService/INDocCopy", ReplyAction="http://tempuri.org/IImageNowService/INDocCopyResponse")]
        byte[] INDocCopy(string doc_id, string authString);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IImageNowServiceChannel : GSA.R7BD.Utility.IImageNowService.IImageNowService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ImageNowServiceClient : System.ServiceModel.ClientBase<GSA.R7BD.Utility.IImageNowService.IImageNowService>, GSA.R7BD.Utility.IImageNowService.IImageNowService {
        
        public ImageNowServiceClient() {
        }
        
        public ImageNowServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ImageNowServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ImageNowServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ImageNowServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string SayHello(string name) {
            return base.Channel.SayHello(name);
        }
        
        public byte[] INDocCopy(string doc_id, string authString) {
            return base.Channel.INDocCopy(doc_id, authString);
        }
    }
}
