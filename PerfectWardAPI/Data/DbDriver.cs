using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PerfectWardAPI.Data
{
    public static class DbDriver
    {
        public static IDbDriver Create(string connStr)
        {
            var driverType = new DirectoryInfo(Environment.CurrentDirectory)
                .GetFiles("*Driver.dll")
                .Select(x =>
                {
                    var assembly = Assembly.LoadFile(x.FullName);
                    return assembly.GetTypes().FirstOrDefault(t => typeof(IDbDriver).IsAssignableFrom(t));
                }).FirstOrDefault();

            if (driverType == null) return null;
            return (IDbDriver)Activator.CreateInstance(driverType, connStr);
        }
    }
}
