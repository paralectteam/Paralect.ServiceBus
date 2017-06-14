﻿using System;
using Microsoft.Practices.ServiceLocation;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public class ObjectHandlerExecutor
    {
        private readonly Type _handlerType;
        private readonly IServiceLocator _serviceLocator;

        public ObjectHandlerExecutor(Type handlerType, IServiceLocator serviceLocator)
        {
            _handlerType = handlerType;
            _serviceLocator = serviceLocator;
        }

        public void Execute(object message)
        {
            var handler = _serviceLocator.GetInstance(_handlerType);

            //handler.GetType().GetMethod()


        }

        public void InvokeDynamic(object handler, object message)
        {
            dynamic dynamicHandler = handler;
            dynamic dynamicMessage = message;

            dynamicHandler.Handle(dynamicMessage);
        }
    }
}