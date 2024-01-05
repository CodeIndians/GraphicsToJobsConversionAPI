using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class S3BucketInfo
{
    public string Id { get; set; }
    public string Label { get; set; }
}

public class APIJobInfo
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
}

public class FlowDiagramParser
{
    public (List<S3BucketInfo>, List<APIJobInfo>) ParseJSONAndCreateJobs(string jsonString)
    {
        try
        {
            // Deserialize the JSON string to a dynamic object
            dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

            // Extract nodes and edges from the JSON structure
            List<S3BucketInfo> nodes = ExtractNodes(jsonData.nodes);
            List<APIJobInfo> Jobs = CreateJobs(jsonData.edges);

            return (nodes, Jobs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
            return (null, null);
        }
    }

    private List<S3BucketInfo> ExtractNodes(dynamic jsonNodes)
    {
        List<S3BucketInfo> nodes = new List<S3BucketInfo>();

        if (jsonNodes != null && jsonNodes.HasValues)
        {
            foreach (var node in jsonNodes)
            {
                string id = node.id?.ToString();
                string label = node.data?.label?.ToString();

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(label))
                {
                    nodes.Add(new S3BucketInfo { Id = id, Label = label });
                }
            }
        }

        return nodes;
    }

    private List<APIJobInfo> CreateJobs(dynamic jsonEdges)
    {
        List<APIJobInfo> edges = new List<APIJobInfo>();

        if (jsonEdges != null && jsonEdges.HasValues)
        {
            // Dictionary to store edges by source
            Dictionary<string, List<APIJobInfo>> edgesBySource = new Dictionary<string, List<APIJobInfo>>();

            foreach (var edge in jsonEdges)
            {
                string id = edge.id?.ToString();
                string source = edge.source?.ToString();
                string target = edge.target?.ToString();

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target))
                {
                    APIJobInfo flowDiagramEdge = new APIJobInfo { Id = id, Source = source, Target = target };

                    // Group edges by source
                    if (!edgesBySource.ContainsKey(source))
                    {
                        edgesBySource[source] = new List<APIJobInfo>();
                    }
                    edgesBySource[source].Add(flowDiagramEdge);
                }
            }

            // Reorder edges sequentially
            foreach (var edgeList in edgesBySource.Values)
            {
                for (int i = 0; i < edgeList.Count; i++)
                {
                    edges.Add(edgeList[i]);
                }
            }
        }

        return edges;
    }
}

class Program
{
    static void Main()
    {
        // Read JSON string from file
        string jsonFilePath = "..\\..\\ReactFlowObject.json";
        string jsonString = File.ReadAllText(jsonFilePath);

        // Create an instance of the FlowDiagramParser
        FlowDiagramParser parser = new FlowDiagramParser();

        // Parse the JSON and get the list of Buckets and Jobs
        var (nodes, jobs) = parser.ParseJSONAndCreateJobs(jsonString);

        // Display the results
        if (jobs != null)
        {

            Console.WriteLine("\nJobs:");
            int jobNumber = 1;

            foreach (var job in jobs)
            {
                Console.WriteLine($"Job Number : {jobNumber++} ; Id: {job.Id}, Source Bucket: {job.Source}, Target Bucket: {job.Target}");
            }
        }
    }
}