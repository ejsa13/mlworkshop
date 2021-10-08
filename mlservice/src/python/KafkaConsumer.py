from confluent_kafka import Consumer
from KafkaEventHandler import handleEvent

import configparser
import logging
import os

def main():
    config = configparser.ConfigParser()
    config.read('config.ini')

    #check if env vars are set
    if "KAFKA__BOOTSTRAPSERVERS" in os.environ:
        config['KAFKA']['BOOTSTRAPSERVERS'] = os.environ['KAFKA__BOOTSTRAPSERVERS']

    if "KAFKA__GROUPID" in os.environ:
        config['KAFKA']['GROUPID'] = os.environ['KAFKA__GROUPID']

    if "KAFKA__ENABLEAUTOCOMMIT" in os.environ:
        config['KAFKA']['ENABLEAUTOCOMMIT'] = os.environ['KAFKA__ENABLEAUTOCOMMIT']

    if "KAFKA__AUTOOFFSETRESET" in os.environ:
        config['KAFKA']['AUTOOFFSETRESET'] = os.environ['KAFKA__AUTOOFFSETRESET']

    if "KAFKA__TOPIC" in os.environ:
        config['KAFKA']['TOPIC'] = os.environ['KAFKA__TOPIC']

    logging.basicConfig(level="DEBUG")

    conf = {'bootstrap.servers': config['KAFKA']['BOOTSTRAPSERVERS'],
    'group.id': config['KAFKA']['GROUPID'],
    'enable.auto.commit': config['KAFKA']['ENABLEAUTOCOMMIT'],
    'auto.offset.reset': config['KAFKA']['AUTOOFFSETRESET']}

    consumer = Consumer(conf)
    consumer.subscribe([config['KAFKA']['TOPIC']])

    try:
        while True:
            msg = consumer.poll(1.0)
            if msg is None:
                logging.debug("waiting for message or event in poll()")
                continue
            elif msg.error():
                logging.error('error: {}'.format(msg.error()))
            else:
                logging.debug('received cloud event: {}'.format(msg.value()))
                handleEvent(msg.value())
                
    except KeyboardInterrupt:
        pass
    finally:
        consumer.close()

if __name__ == '__main__':
    main()