// <auto-generated/>

#nullable disable

using System;
using System.ClientModel.Primitives;

namespace Sample
{
    public partial class TestClient
    {
        internal global::System.ClientModel.Primitives.PipelineMessage CreateGetCatsRequest(string p1, string p2, string p3, global::System.ClientModel.Primitives.RequestOptions options)
        {
            global::System.ClientModel.Primitives.PipelineMessage message = Pipeline.CreateMessage();
            message.ResponseClassifier = PipelineMessageClassifier200;
            global::System.ClientModel.Primitives.PipelineRequest request = message.Request;
            request.Method = "GET";
            global::Sample.ClientUriBuilder uri = new global::Sample.ClientUriBuilder();
            uri.Reset(_endpoint);
            uri.AppendQuery("someOtherName", p1, true);
            uri.AppendQuery("p2", p2, true);
            request.Uri = uri.ToUri();
            request.Headers.Set("p3", p3);
            request.Headers.Set("Accept", "application/json");
            message.Apply(options);
            return message;
        }

        internal global::System.ClientModel.Primitives.PipelineMessage CreateNextGetCatsRequest(global::System.Uri nextPage, string p1, string p2, string p3, global::System.ClientModel.Primitives.RequestOptions options)
        {
            global::System.ClientModel.Primitives.PipelineMessage message = Pipeline.CreateMessage();
            message.ResponseClassifier = PipelineMessageClassifier200;
            global::System.ClientModel.Primitives.PipelineRequest request = message.Request;
            request.Method = "GET";
            global::Sample.ClientUriBuilder uri = new global::Sample.ClientUriBuilder();
            uri.Reset(nextPage);
            uri.AppendQuery("someOtherName", p1, true);
            uri.AppendQuery("p2", p2, true);
            request.Uri = uri.ToUri();
            request.Headers.Set("p3", p3);
            request.Headers.Set("Accept", "application/json");
            message.Apply(options);
            return message;
        }
    }
}
