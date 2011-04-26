﻿using System.Linq;
using System.Reflection;
using System.Web.Routing;

namespace tinyweb.framework
{
    public class DefaultFilterInvoker : IFilterInvoker
    {
        readonly IArgumentBuilder _argumentBuilder;

        public DefaultFilterInvoker(IArgumentBuilder argumentBuilder)
        {
            _argumentBuilder = argumentBuilder;
        }

        public IResult RunBefore(object filter, RequestContext requestContext)
        {
            return executeFilterMethod(filter, "Before", requestContext);
        }

        public IResult RunAfter(object filter, RequestContext requestContext)
        {
            return executeFilterMethod(filter, "After", requestContext);
        }

        private IResult executeFilterMethod(object filter, string filterMethod, RequestContext context)
        {
            var method = getMethod(filter, filterMethod);

            if (method != null)
            {
                var args = _argumentBuilder.BuildArguments(method.GetParameters(), context);
                return method.Invoke(filter, args) as IResult;
            }

            return null;
        }

        private MethodInfo getMethod(object filter, string name)
        {
            return filter.GetType().GetMethods().SingleOrDefault(m => m.Name.ToLower() == name.ToLower());
        }
    }
}