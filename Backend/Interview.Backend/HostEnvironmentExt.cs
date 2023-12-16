namespace Interview.Backend
{
    public static class HostEnvironmentExt
    {
        public static bool IsPreProduction(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);
            return hostEnvironment.IsEnvironment("PreProduction");
        }
    }
}
