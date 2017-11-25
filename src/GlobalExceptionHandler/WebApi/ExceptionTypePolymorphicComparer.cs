using System;
using System.Collections.Generic;

namespace GlobalExceptionHandler.WebApi
{
    internal class ExceptionTypePolymorphicComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            var depthOfX = 0;
            var currentType = x;
            while (currentType != typeof(object))
            {
                currentType = currentType.BaseType;
                depthOfX++;
            }

            var depthOfY = 0;
            currentType = y;
            while (currentType != typeof(object))
            {
                currentType = currentType.BaseType;
                depthOfY++;
            }

            return depthOfX.CompareTo(depthOfY);
        }
    }
}