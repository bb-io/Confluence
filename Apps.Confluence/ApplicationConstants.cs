namespace Apps.Confluence;

public class ApplicationConstants
{
    public static string ClientId => "#{CONFLUENCE_CLIENT_ID}#";
    
    public static string ClientSecret => "#{CONFLUENCE_CLIENT_SECRET}#"; 
    
    public static string Scope => "write:confluence-content read:confluence-space.summary write:confluence-space write:confluence-file read:confluence-props write:confluence-props manage:confluence-configuration read:confluence-content.all search:confluence read:confluence-content.summary read:confluence-content.permission read:confluence-user read:confluence-groups write:confluence-groups readonly:content.attachment:confluence read:content-details:confluence read:content:confluence write:content:confluence delete:content:confluence read:attachment:confluence read:custom-content:confluence write:custom-content:confluence delete:custom-content:confluence read:template:confluence write:template:confluence read:content.permission:confluence read:content.property:confluence write:content.property:confluence read:space:confluence read:space-details:confluence offline_access";
}