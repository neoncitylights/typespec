package authentication.union;

import authentication.union.implementation.UnionClientImpl;
import io.clientcore.core.annotations.Metadata;
import io.clientcore.core.annotations.MetadataProperties;
import io.clientcore.core.annotations.ReturnType;
import io.clientcore.core.annotations.ServiceClient;
import io.clientcore.core.annotations.ServiceMethod;
import io.clientcore.core.http.models.HttpResponseException;
import io.clientcore.core.http.models.RequestContext;
import io.clientcore.core.http.models.Response;

/**
 * Initializes a new instance of the synchronous UnionClient type.
 */
@ServiceClient(builder = UnionClientBuilder.class)
public final class UnionClient {
    @Metadata(properties = { MetadataProperties.GENERATED })
    private final UnionClientImpl serviceClient;

    /**
     * Initializes an instance of UnionClient class.
     * 
     * @param serviceClient the service client implementation.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    UnionClient(UnionClientImpl serviceClient) {
        this.serviceClient = serviceClient;
    }

    /**
     * Check whether client is authenticated.
     * 
     * @param requestContext The context to configure the HTTP request before HTTP client sends it.
     * @throws IllegalArgumentException thrown if parameters fail the validation.
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     * @return the response.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public Response<Void> validKeyWithResponse(RequestContext requestContext) {
        return this.serviceClient.validKeyWithResponse(requestContext);
    }

    /**
     * Check whether client is authenticated.
     * 
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public void validKey() {
        validKeyWithResponse(RequestContext.none());
    }

    /**
     * Check whether client is authenticated.
     * 
     * @param requestContext The context to configure the HTTP request before HTTP client sends it.
     * @throws IllegalArgumentException thrown if parameters fail the validation.
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     * @return the response.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public Response<Void> validTokenWithResponse(RequestContext requestContext) {
        return this.serviceClient.validTokenWithResponse(requestContext);
    }

    /**
     * Check whether client is authenticated.
     * 
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public void validToken() {
        validTokenWithResponse(RequestContext.none());
    }
}
