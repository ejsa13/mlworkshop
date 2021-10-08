package org.acme;

import java.util.HashMap;

import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.json.bind.JsonbBuilder;

import org.eclipse.microprofile.rest.client.inject.RestClient;
import org.jboss.logging.Logger;

@ApplicationScoped
public class KafkaEventHandler {
    
    @Inject
    @RestClient
    DriftDetectorService driftService;

    @Inject
    @RestClient
    OutlierDetectorService outlierService;

    private final Logger logger = Logger.getLogger(KafkaConsumer.class);
    
    public void handle(String message){
        var parser = JsonbBuilder.create();
        var obj = parser.fromJson(message, HashMap.class);
        var payload = obj.get("body").toString();
        logger.debugf("calling outlier detector", "");
        var outlierResponse = outlierService.callPost(payload);
        logger.infof("{\"detector\":\"outlier\", \"request\": %s, \"payload\":%s, \"response\":{\"StatusCode\":%s, \"Content\":%s}}", 
            message, payload, outlierResponse.getStatus(), outlierResponse.readEntity(Integer.class));

        logger.debugf("calling drift detector", "");
        var driftResponse = driftService.callPost(payload);
        logger.infof("{\"detector\":\"drift\", \"request\": %s, \"payload\":%s, \"response\":{\"StatusCode\":%s, \"Content\":%s}}", 
            message, payload, driftResponse.getStatus(), driftResponse.readEntity(Integer.class));

    }
}
