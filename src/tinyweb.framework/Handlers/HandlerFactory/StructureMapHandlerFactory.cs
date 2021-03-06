﻿using StructureMap;

namespace tinyweb.framework
{
    public class StructureMapHandlerFactory : IHandlerFactory
    {
        public object Create(HandlerData handlerData)
        {
            return ObjectFactory.GetInstance(handlerData.Type);
        }
    }
}