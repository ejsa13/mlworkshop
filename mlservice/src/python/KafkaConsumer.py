from confluent_kafka import Consumer
from KafkaEventHandler import handleEvent

import logging

def main():
    logging.basicConfig(level="DEBUG")

    conf = {'bootstrap.servers': 'localhost:9092',
    'group.id': "foo",
    'enable.auto.commit': False,
    'auto.offset.reset': 'earliest'}

    consumer = Consumer(conf)
    consumer.subscribe(['mlworkshop'])

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