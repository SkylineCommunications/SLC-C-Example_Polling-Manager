namespace Skyline.Protocol.PollingManager.CustomCode.Configuration
{
	using System.Collections.Generic;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	public class PollingManagerConfiguration : PollingManagerConfigurationBase
    {
        public PollingManagerConfiguration(SLProtocol protocol) : base(protocol)
        {
            Rows = new Dictionary<string, PollableBase>()
            {
                { "Basic", new BasicPoll(Protocol, "Basic Dataset", 60_001) },
                { "Fail", new BasicPoll(Protocol, "Failing Dataset", 60_002) },
                { "Parent1", new Pollable(Protocol, "Parent1 Dataset") },
                { "Parent2", new Pollable(Protocol, "Parent2 Dataset") },
                { "Child1", new Pollable(Protocol, "Parent1 - Child 1 Dataset") },
                { "Child2", new Pollable(Protocol, "Multiple Parent - Child 2 Dataset") },
                { "Dependency", new Pollable(Protocol, "API 2.0 Dataset") },
                { "Mandatory", new Pollable(Protocol, "Mandatory Dataset") },
            };

            Rows["Mandatory"].Mandatory = true;

            Dependencies = new List<Dependency>()
            {
            };

            ResponseHandlers = new Dictionary<int, ResponseHandler>()
            { };
        }

        public override Dictionary<int, ResponseHandler> ResponseHandlers { get; set; }

        protected override List<Dependency> Dependencies { get; set; }

        protected override Dictionary<string, PollableBase> Rows { get; set; }

        protected override void CreateDependencies()
        {
            var apiDependency = new Dependency("Version2", true, "Only supported in Api version 2.0");
            Rows["Dependency"].AddDependency(Parameter.apiversion_5, apiDependency);
        }

        protected override void CreateParameterRelations()
        {
            var basicSingleParameter = new Dictionary<int, object>
            {
                { Parameter.systemname_20, string.Empty},
                { Parameter.serialNumber_21,"-1"},
            };

            var basicTableParameters = new List<int> { 100 };
            Rows["Basic"].AddParameters(basicSingleParameter, basicTableParameters);
        }

        protected override void CreateRelations()
        {
            Rows["Parent1"].AddChildren(Rows["Child1"], Rows["Child2"]);
            Rows["Parent2"].AddChildren(Rows["Child2"]);
        }

        protected override void CreateResponseHandlers()
        {
            ResponseHandlers.Add(61001, new ResponseBasicDataSet("Basic"));
            ResponseHandlers.Add(61002, new ResponseBasicFailDataSet("Fail"));
        }
    }
}
