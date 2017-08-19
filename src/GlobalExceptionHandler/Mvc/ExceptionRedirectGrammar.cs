using System;
using System.Collections.Concurrent;

namespace GlobalExceptionHandler.Mvc
{
    public class ExceptionRedirectGrammar : ICanRedirect, IRedirectType
    {
        private readonly ConcurrentDictionary<Type, RerouteLocation> _configuration;
        private readonly Type _type;

        public ExceptionRedirectGrammar(ConcurrentDictionary<Type, RerouteLocation> configuration, Type type)
        {
            _configuration = configuration;
            _type = type;
        }

        public IRedirectType RedirectTo()
        {
            return this;
        }

        public void Path(string pathName)
        {
            _configuration.TryAdd(_type, new RerouteLocation
            {
                PathName = pathName,
                RedirectionType = RedirectionType.PathName
            });
        }

        public void RouteName(string routeName)
        {
            _configuration.TryAdd(_type, new RerouteLocation
            {
                RouteName = routeName,
                RedirectionType = RedirectionType.RouteName
            });
        }

        public void Action(string controllerName, string action)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICanRedirect
    {
        IRedirectType RedirectTo();
    }

    public interface IRedirectType
    {
        void Path(string pathName);

        void RouteName(string routeName);

        void Action(string controllerName, string action);
    }
}