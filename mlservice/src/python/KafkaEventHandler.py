from pygelf import GelfUdpHandler

import configparser
import json
import logging
import os
import requests

config = configparser.ConfigParser()
config.read('config.ini')

#check if env vars are set
if "GELF__HOST" in os.environ:
    config['GELF']['HOST'] = os.environ['GELF__HOST']

if "GELF__PORT" in os.environ:
    config['GELF']['PORT'] = os.environ['GELF__PORT']

if "DETECTOR__DRIFTURL" in os.environ:
    config['DETECTOR']['DRIFTURL'] = os.environ['DETECTOR__DRIFTURL']

if "DETECTOR__OUTLIERURL" in os.environ:
    config['DETECOR']['OUTLIERURL'] = os.environ['DETECTOR__OUTLIERURL']

logger = logging.getLogger(__name__)
logger.addHandler(GelfUdpHandler(host=config['GELF']['HOST'], port=int(config['GELF']['PORT'])))

def handleEvent(message):
    logger.debug('handling event: {}'.format(message))

    callDetector('drift', config['DETECTOR']['DRIFTURL'], message)
    callDetector('outlier', config['DETECTOR']['OUTLIERURL'], message)
    
def callDetector(name, url, message):
    jsonMsg = json.loads(message)
    payload = jsonMsg["body"]    
    logger.debug('payload: {}'.format(payload))
    logger.debug('calling {} detector'.format(name))
    response = requests.post(url, json=payload, verify=False)
    logger.info('{{detector":{0}, "request":{1}, "payload":{2}, "response": {{"StatusCode":{3}, "Content":{4}}}}}'
            .format(name, jsonMsg, payload, response.status_code, response.json()))
