import json
import logging
import requests

logger = logging.getLogger(__name__)

def handleEvent(message):
    logger.debug('handling event: {}'.format(message))

    callDetector('drift', "https://localhost:6001/weatherforecast", message)
    callDetector('outlier', "https://localhost:6001/weatherforecast", message)
    
def callDetector(name, url, message):
    jsonMsg = json.loads(message)
    payload = jsonMsg["body"]    
    logger.debug('payload: {}'.format(payload))
    logger.debug('calling {} detector'.format(name))
    response = requests.post(url, json=payload, verify=False)
    logger.info('{{detector":{0}, "request":{1}, "payload":{2}, "response": {{"StatusCode":{3}, "Content":{4}}}}}'
            .format(name, message, payload, response.status_code, response.json()))
