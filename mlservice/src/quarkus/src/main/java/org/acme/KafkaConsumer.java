package org.acme;

import io.smallrye.reactive.messaging.kafka.Record;
import org.eclipse.microprofile.reactive.messaging.Incoming;
import org.jboss.logging.Logger;

import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.validation.constraints.Null;

@ApplicationScoped
public class KafkaConsumer {

    @Inject
    KafkaEventHandler handler;
    private final Logger logger = Logger.getLogger(KafkaConsumer.class);

    @Incoming("mlworkshop")
    public void receive(Record<Null, String> record){
        logger.debugf("received cloud event: %s", record.value());
        handler.handle(record.value());
    }
}
