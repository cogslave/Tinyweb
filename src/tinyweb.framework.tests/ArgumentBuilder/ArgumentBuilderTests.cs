using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Web.Routing;
using Moq;
using System.Web;
using System.Collections.Specialized;

namespace tinyweb.framework.tests
{
    [TestFixture]
    class ArgumentBuilderTests
    {
        IArgumentBuilder argumentBuilder;
        ParameterProvider parameterProvider;
        Mock<HttpContextBase> httpContext;
        Mock<HttpRequestBase> request;

        [SetUp]
        public void Setup()
        {
            parameterProvider = new ParameterProvider();
            argumentBuilder = new ArgumentBuilder();
            httpContext = new Mock<HttpContextBase>();
            request = new Mock<HttpRequestBase>();

            httpContext.Setup(x => x.Request).Returns(request.Object);
        }

        private class ParameterClass
        {
            public bool on { get; set; }
            public bool off { get; set; }
            public string text { get; set; }
            public int number { get; set; }
        }

        private class ParameterProvider
        {
            public void NoParameters(){}
            public void UsesAnArray(int[] numbers) { }
            public void OptionalParameters(string setme, bool on = true, bool off = false, string text = "hello", int number = -99) { }
            public void ClassParameter(ParameterClass argument) { }

            public ParameterInfo[] GetParamters(string methodName)
            {
                MethodInfo method = GetMethod(this, methodName);
                return method.GetParameters();
            }

            private MethodInfo GetMethod(object item, string name)
            {
                return item.GetType().GetMethods().SingleOrDefault(m => m.Name.ToLower() == name.ToLower());
            }
        }

        [Test]
        public void Build_MissingParameters()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection());
            request.Setup(x => x.Form).Returns(new NameValueCollection());

            Assert.Throws(typeof(Exception),
                () => argumentBuilder.BuildArguments(parameterProvider.GetParamters("OptionalParameters"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData())
            );
        }

        [Test]
        public void Build_CreatesNoParameters()
        {
            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("NoParameters"), new Mock<RequestContext>().Object, new HandlerData());
            Assert.That(arguments.Length, Is.EqualTo(0));
        }

        [Test]
        public void Build_UsesDefaultsForMissingOptionalParameters()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection() { { "setme", "test" } });
            request.Setup(x => x.Form).Returns(new NameValueCollection());

            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("OptionalParameters"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());

            Assert.That(arguments[0], Is.EqualTo("test"));
            Assert.That(arguments[1], Is.EqualTo(true));
            Assert.That(arguments[2], Is.EqualTo(false));
            Assert.That(arguments[3], Is.EqualTo("hello"));
            Assert.That(arguments[4], Is.EqualTo(-99));
        }

        [Test]
        public void Build_UsesQueryStringValuesForParameters()
        {          
            request.Setup(x => x.QueryString).Returns(new NameValueCollection() { { "setme", "test" }, { "on", "false" }, { "off", "true" }, { "text", "goodbye" }, { "number", "100" } });
            request.Setup(x => x.Form).Returns(new NameValueCollection());

            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("OptionalParameters"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());

            Assert.That(arguments[0], Is.EqualTo("test"));
            Assert.That(arguments[1], Is.EqualTo(false));
            Assert.That(arguments[2], Is.EqualTo(true));
            Assert.That(arguments[3], Is.EqualTo("goodbye"));
            Assert.That(arguments[4], Is.EqualTo(100));
        }

        [Test]
        public void Build_UsesFormValuesForParameters()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection());
            request.Setup(x => x.Form).Returns(new NameValueCollection() { { "setme", "test" }, { "on", "false" }, { "off", "true" }, { "text", "goodbye" }, { "number", "100" } });

            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("OptionalParameters"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());

            Assert.That(arguments[0], Is.EqualTo("test"));
            Assert.That(arguments[1], Is.EqualTo(false));
            Assert.That(arguments[2], Is.EqualTo(true));
            Assert.That(arguments[3], Is.EqualTo("goodbye"));
            Assert.That(arguments[4], Is.EqualTo(100));
        }

        [Test]
        public void Build_WithClassFromQueryString_UsesClass()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection() { { "setme", "test" }, { "on", "false" }, { "off", "true" }, { "text", "goodbye" }, { "number", "100" } });
            request.Setup(x => x.Form).Returns(new NameValueCollection());

            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("ClassParameter"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());
            ParameterClass parameters = (ParameterClass)arguments[0];

            Assert.That(parameters.on, Is.EqualTo(false));
            Assert.That(parameters.off, Is.EqualTo(true));
            Assert.That(parameters.text, Is.EqualTo("goodbye"));
            Assert.That(parameters.number, Is.EqualTo(100));
        }

        [Test]
        public void Build_WithClassFromForm_UsesClass()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection());
            request.Setup(x => x.Form).Returns(new NameValueCollection() { { "setme", "test" }, { "on", "false" }, { "off", "true" }, { "text", "goodbye" }, { "number", "100" } });
            
            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("ClassParameter"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());
            ParameterClass parameters = (ParameterClass)arguments[0];

            Assert.That(parameters.on, Is.EqualTo(false));
            Assert.That(parameters.off, Is.EqualTo(true));
            Assert.That(parameters.text, Is.EqualTo("goodbye"));
            Assert.That(parameters.number, Is.EqualTo(100));
        }

        [Test]
        public void Build_WithArrayFromForm_CreatesAnArray()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection());
            request.Setup(x => x.Form).Returns(new NameValueCollection() { { "numbers[]", "1" }, { "numbers[]", "2" }, { "numbers[]", "3" }, { "numbers[]", "4" }, { "numbers[]", "5" } });

            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("UsesAnArray"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());

            int[] numbers = (int[])arguments[0];

            Assert.That(numbers.Length, Is.EqualTo(5));
            Assert.That(numbers[0], Is.EqualTo(1));
            Assert.That(numbers[1], Is.EqualTo(2));
            Assert.That(numbers[2], Is.EqualTo(3));
            Assert.That(numbers[3], Is.EqualTo(4));
            Assert.That(numbers[4], Is.EqualTo(5));
        }

        [Test]
        public void Build_WithArrayFromQueryString_CreatesAnArray()
        {
            request.Setup(x => x.QueryString).Returns(new NameValueCollection() { { "numbers[]", "1" }, { "numbers[]", "2" }, { "numbers[]", "3" }, { "numbers[]", "4" }, { "numbers[]", "5" } });
            request.Setup(x => x.Form).Returns(new NameValueCollection());

            object[] arguments = argumentBuilder.BuildArguments(parameterProvider.GetParamters("UsesAnArray"), new RequestContext(httpContext.Object, new RouteData()), new HandlerData());

            int[] numbers = (int[])arguments[0];

            Assert.That(numbers.Length, Is.EqualTo(5));
            Assert.That(numbers[0], Is.EqualTo(1));
            Assert.That(numbers[1], Is.EqualTo(2));
            Assert.That(numbers[2], Is.EqualTo(3));
            Assert.That(numbers[3], Is.EqualTo(4));
            Assert.That(numbers[4], Is.EqualTo(5));
        }
    }
}