// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.ServiceModel.Description
{
    using System.IO;
    using System.ServiceModel.Channels;
    using Microsoft.Xml;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using WsdlNS = System.Web.Services.Description;

    public class WsdlEndpointConversionContext
    {
        private readonly ServiceEndpoint _endpoint;
        private readonly WsdlNS.Binding _wsdlBinding;
        private readonly WsdlNS.Port _wsdlPort;
        private readonly WsdlContractConversionContext _contractContext;

        private readonly Dictionary<OperationDescription, WsdlNS.OperationBinding> _wsdlOperationBindings;
        private readonly Dictionary<WsdlNS.OperationBinding, OperationDescription> _operationDescriptionBindings;
        private readonly Dictionary<MessageDescription, WsdlNS.MessageBinding> _wsdlMessageBindings;
        private readonly Dictionary<FaultDescription, WsdlNS.FaultBinding> _wsdlFaultBindings;
        private readonly Dictionary<WsdlNS.MessageBinding, MessageDescription> _messageDescriptionBindings;
        private readonly Dictionary<WsdlNS.FaultBinding, FaultDescription> _faultDescriptionBindings;

        internal WsdlEndpointConversionContext(WsdlContractConversionContext contractContext, ServiceEndpoint endpoint, WsdlNS.Binding wsdlBinding, WsdlNS.Port wsdlport)
        {
            _endpoint = endpoint;
            _wsdlBinding = wsdlBinding;
            _wsdlPort = wsdlport;
            _contractContext = contractContext;

            _wsdlOperationBindings = new Dictionary<OperationDescription, WsdlNS.OperationBinding>();
            _operationDescriptionBindings = new Dictionary<WsdlNS.OperationBinding, OperationDescription>();
            _wsdlMessageBindings = new Dictionary<MessageDescription, WsdlNS.MessageBinding>();
            _messageDescriptionBindings = new Dictionary<WsdlNS.MessageBinding, MessageDescription>();
            _wsdlFaultBindings = new Dictionary<FaultDescription, WsdlNS.FaultBinding>();
            _faultDescriptionBindings = new Dictionary<WsdlNS.FaultBinding, FaultDescription>();
        }

        internal WsdlEndpointConversionContext(WsdlEndpointConversionContext bindingContext, ServiceEndpoint endpoint, WsdlNS.Port wsdlport)
        {
            _endpoint = endpoint;
            _wsdlBinding = bindingContext.WsdlBinding;
            _wsdlPort = wsdlport;
            _contractContext = bindingContext._contractContext;

            _wsdlOperationBindings = bindingContext._wsdlOperationBindings;
            _operationDescriptionBindings = bindingContext._operationDescriptionBindings;
            _wsdlMessageBindings = bindingContext._wsdlMessageBindings;
            _messageDescriptionBindings = bindingContext._messageDescriptionBindings;
            _wsdlFaultBindings = bindingContext._wsdlFaultBindings;
            _faultDescriptionBindings = bindingContext._faultDescriptionBindings;
        }

        internal IEnumerable<IWsdlExportExtension> ExportExtensions
        {
            get
            {
                foreach (IWsdlExportExtension extension in _endpoint.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (IWsdlExportExtension extension in _endpoint.Binding.CreateBindingElements().FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (IWsdlExportExtension extension in _endpoint.Contract.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (OperationDescription operation in _endpoint.Contract.Operations)
                {
                    if (!WsdlExporter.OperationIsExportable(operation))
                    {
                        continue;
                    }

                    // In 3.0SP1, the DCSOB and XSOB were moved from before to after the custom behaviors.  For
                    // IWsdlExportExtension compat, run them in the pre-SP1 order.
                    // TEF QFE 367607
                    Collection<IWsdlExportExtension> extensions = operation.Behaviors.FindAll<IWsdlExportExtension>();
                    for (int i = 0; i < extensions.Count;)
                    {
                        if (WsdlExporter.IsBuiltInOperationBehavior(extensions[i]))
                        {
                            yield return extensions[i];
                            extensions.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    foreach (IWsdlExportExtension extension in extensions)
                    {
                        yield return extension;
                    }
                }
            }
        }

        public ServiceEndpoint Endpoint { get { return _endpoint; } }
        public WsdlNS.Binding WsdlBinding { get { return _wsdlBinding; } }
        public WsdlNS.Port WsdlPort { get { return _wsdlPort; } }
        public WsdlContractConversionContext ContractConversionContext { get { return _contractContext; } }

        public WsdlNS.OperationBinding GetOperationBinding(OperationDescription operation)
        {
            return _wsdlOperationBindings[operation];
        }

        public WsdlNS.MessageBinding GetMessageBinding(MessageDescription message)
        {
            return _wsdlMessageBindings[message];
        }

        public WsdlNS.FaultBinding GetFaultBinding(FaultDescription fault)
        {
            return _wsdlFaultBindings[fault];
        }

        public OperationDescription GetOperationDescription(WsdlNS.OperationBinding operationBinding)
        {
            return _operationDescriptionBindings[operationBinding];
        }

        public MessageDescription GetMessageDescription(WsdlNS.MessageBinding messageBinding)
        {
            return _messageDescriptionBindings[messageBinding];
        }

        public FaultDescription GetFaultDescription(WsdlNS.FaultBinding faultBinding)
        {
            return _faultDescriptionBindings[faultBinding];
        }

        // --------------------------------------------------------------------------------------------------

        internal void AddOperationBinding(OperationDescription operationDescription, WsdlNS.OperationBinding wsdlOperationBinding)
        {
            _wsdlOperationBindings.Add(operationDescription, wsdlOperationBinding);
            _operationDescriptionBindings.Add(wsdlOperationBinding, operationDescription);
        }

        internal void AddMessageBinding(MessageDescription messageDescription, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            _wsdlMessageBindings.Add(messageDescription, wsdlMessageBinding);
            _messageDescriptionBindings.Add(wsdlMessageBinding, messageDescription);
        }

        internal void AddFaultBinding(FaultDescription faultDescription, WsdlNS.FaultBinding wsdlFaultBinding)
        {
            _wsdlFaultBindings.Add(faultDescription, wsdlFaultBinding);
            _faultDescriptionBindings.Add(wsdlFaultBinding, faultDescription);
        }
    }
}
