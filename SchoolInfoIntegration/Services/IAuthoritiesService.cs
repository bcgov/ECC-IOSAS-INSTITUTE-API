namespace ECC.Institute.CRM.IntegrationAPI
{
    public interface IAuthoritiesService
    {
        Boolean upsert(string applicationName, dynamic payload);
    }
}