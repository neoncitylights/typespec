package type.scalar;

import io.clientcore.core.annotations.Metadata;
import io.clientcore.core.annotations.MetadataProperties;
import io.clientcore.core.annotations.ReturnType;
import io.clientcore.core.annotations.ServiceClient;
import io.clientcore.core.annotations.ServiceMethod;
import io.clientcore.core.http.models.HttpResponseException;
import io.clientcore.core.http.models.RequestContext;
import io.clientcore.core.http.models.Response;
import java.math.BigDecimal;
import java.util.List;
import type.scalar.implementation.DecimalVerifiesImpl;

/**
 * Initializes a new instance of the synchronous ScalarClient type.
 */
@ServiceClient(builder = ScalarClientBuilder.class)
public final class DecimalVerifyClient {
    @Metadata(properties = { MetadataProperties.GENERATED })
    private final DecimalVerifiesImpl serviceClient;

    /**
     * Initializes an instance of DecimalVerifyClient class.
     * 
     * @param serviceClient the service client implementation.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    DecimalVerifyClient(DecimalVerifiesImpl serviceClient) {
        this.serviceClient = serviceClient;
    }

    /**
     * The prepareVerify operation.
     * 
     * @param requestContext The context to configure the HTTP request before HTTP client sends it.
     * @throws IllegalArgumentException thrown if parameters fail the validation.
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     * @return the response.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public Response<List<BigDecimal>> prepareVerifyWithResponse(RequestContext requestContext) {
        return this.serviceClient.prepareVerifyWithResponse(requestContext);
    }

    /**
     * The prepareVerify operation.
     * 
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     * @return the response.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public List<BigDecimal> prepareVerify() {
        return prepareVerifyWithResponse(RequestContext.none()).getValue();
    }

    /**
     * The verify operation.
     * 
     * @param body The body parameter.
     * @param requestContext The context to configure the HTTP request before HTTP client sends it.
     * @throws IllegalArgumentException thrown if parameters fail the validation.
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     * @return the response.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public Response<Void> verifyWithResponse(BigDecimal body, RequestContext requestContext) {
        return this.serviceClient.verifyWithResponse(body, requestContext);
    }

    /**
     * The verify operation.
     * 
     * @param body The body parameter.
     * @throws IllegalArgumentException thrown if parameters fail the validation.
     * @throws HttpResponseException thrown if the service returns an error.
     * @throws RuntimeException all other wrapped checked exceptions if the request fails to be sent.
     */
    @Metadata(properties = { MetadataProperties.GENERATED })
    @ServiceMethod(returns = ReturnType.SINGLE)
    public void verify(BigDecimal body) {
        verifyWithResponse(body, RequestContext.none());
    }
}
