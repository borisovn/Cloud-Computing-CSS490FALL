<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="PaaS_Cloud_Service" generation="1" functional="0" release="0" Id="8debd950-63ff-45ea-a4a4-950c852a3704" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="PaaS_Cloud_ServiceGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="MoviesWebApp:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/LB:MoviesWebApp:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="MoviesWebApp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWebApp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="MoviesWebApp:StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWebApp:StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="MoviesWebAppInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWebAppInstances" />
          </maps>
        </aCS>
        <aCS name="MoviesWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="MoviesWorker:moviesassignment03DbConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWorker:moviesassignment03DbConnectionString" />
          </maps>
        </aCS>
        <aCS name="MoviesWorker:StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWorker:StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="MoviesWorker:StorageConnectionStringBus" defaultValue="">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWorker:StorageConnectionStringBus" />
          </maps>
        </aCS>
        <aCS name="MoviesWorkerInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MapMoviesWorkerInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:MoviesWebApp:Endpoint1">
          <toPorts>
            <inPortMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebApp/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapMoviesWebApp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebApp/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapMoviesWebApp:StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebApp/StorageConnectionString" />
          </setting>
        </map>
        <map name="MapMoviesWebAppInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebAppInstances" />
          </setting>
        </map>
        <map name="MapMoviesWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorker/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapMoviesWorker:moviesassignment03DbConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorker/moviesassignment03DbConnectionString" />
          </setting>
        </map>
        <map name="MapMoviesWorker:StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorker/StorageConnectionString" />
          </setting>
        </map>
        <map name="MapMoviesWorker:StorageConnectionStringBus" kind="Identity">
          <setting>
            <aCSMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorker/StorageConnectionStringBus" />
          </setting>
        </map>
        <map name="MapMoviesWorkerInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorkerInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="MoviesWebApp" generation="1" functional="0" release="0" software="C:\Users\Nikita\Desktop\UW_Classes\CSS490_Fall_2015\Assigment03\Cloud-Computing-CSS490FALL\PaaS_Cloud_Service\PaaS_Cloud_Service\csx\Debug\roles\MoviesWebApp" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="StorageConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;MoviesWebApp&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;MoviesWebApp&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;MoviesWorker&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebAppInstances" />
            <sCSPolicyUpdateDomainMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebAppUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebAppFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
        <groupHascomponents>
          <role name="MoviesWorker" generation="1" functional="0" release="0" software="C:\Users\Nikita\Desktop\UW_Classes\CSS490_Fall_2015\Assigment03\Cloud-Computing-CSS490FALL\PaaS_Cloud_Service\PaaS_Cloud_Service\csx\Debug\roles\MoviesWorker" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="moviesassignment03DbConnectionString" defaultValue="" />
              <aCS name="StorageConnectionString" defaultValue="" />
              <aCS name="StorageConnectionStringBus" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;MoviesWorker&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;MoviesWebApp&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;MoviesWorker&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="InstallLogs" defaultAmount="[5,5,5]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorkerInstances" />
            <sCSPolicyUpdateDomainMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorkerUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWorkerFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="MoviesWebAppUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyUpdateDomain name="MoviesWorkerUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="MoviesWebAppFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyFaultDomain name="MoviesWorkerFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="MoviesWebAppInstances" defaultPolicy="[1,1,1]" />
        <sCSPolicyID name="MoviesWorkerInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="25786e28-5585-40bc-b993-c2b6005286f9" ref="Microsoft.RedDog.Contract\ServiceContract\PaaS_Cloud_ServiceContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="5fd8c75a-7469-4d9f-98c9-59f142bee25a" ref="Microsoft.RedDog.Contract\Interface\MoviesWebApp:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/PaaS_Cloud_Service/PaaS_Cloud_ServiceGroup/MoviesWebApp:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>