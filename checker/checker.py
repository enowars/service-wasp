import time
import logging
import sys
import aiohttp
import random
import string
import json

from enochecker_async import BaseChecker, BrokenServiceException, create_app, OfflineException, ELKFormatter, CheckerTaskMessage
from logging import LoggerAdapter
from motor import MotorCollection

class WaspChecker(BaseChecker):
    port = 8000

    def __init__(self):
        super(WaspChecker, self).__init__("WASP", 8080, 2, 0, 0)

    async def putflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        tag = ''.join(random.choice(string.ascii_uppercase + string.digits) for _ in range(10))
        await collection.insert_one({ 'flag' : task.flag, 'tag': tag })

        logger.debug("Putting Flag...")
        async with aiohttp.ClientSession(raise_for_status=True) as session:
            # / because why not
            try:
                await session.get("http://" + task.address + ":" + str(WaspChecker.port))
            except Exception:
                raise OfflineException()
            if task.flagIndex % 2 == 0:
                attack = {
                    "date": task.flag,
                    "location": "Berlin",
                    "description": "tasty bee hive! #{}".format(tag),
                    "password": task.flag
                }
            else:
                attack = {
                    "date": "whenever",
                    "location": task.flag,
                    "description": "tasty bee hive! #{}".format(tag),
                    "password": task.flag
                }

            # /AddAttack
            try:
                await session.post("http://" + task.address + ":" + str(WaspChecker.port) + "/api/AddAttack", data=attack)
            except:
                raise BrokenServiceException("/AddAttack failed")
            logger.debug("Flag {} up with tag: {}.".format(task.flag, tag))

    async def getflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        async with aiohttp.ClientSession(raise_for_status=True) as session:
            tag = await collection.find_one({ 'flag': task.flag })
            if tag is None:
                raise BrokenServiceException("Could not find tag in db")
            tag = tag["tag"]

            logger.info(f"GET /api/SearchAttacks needle={tag}")
            try:
                search_result = None
                r = await session.get("http://" + task.address + ":" + str(WaspChecker.port) + "/api/SearchAttacks", params={ "needle": tag})
                search_result = await r.text()
                search_results = json.loads(search_result)
                attack_id = search_results["matches"][0]["id"]
            except:
                raise BrokenServiceException(f"Invalid search response: {search_result}")

            logger.info(f"Fetching attack id={attack_id} password={task.flag}")
            try:
                get_result = None
                r = await session.get("http://" + task.address + ":" + str(WaspChecker.port) + "/api/GetAttack", params={"id": attack_id, "password": task.flag}, timeout=5)
                get_result = await r.text()
                matches = json.loads(get_result)
                flag_field = "attackDate" if task.flagIndex % 2 == 0 else "location"
                found_flag = matches["attack"][flag_field]
            except:
                raise BrokenServiceException(f"Invalid get response: {get_result}")
            if found_flag != task.flag:
                raise BrokenServiceException(
                    "Incorrect flag in date field (searched for {} in {} - {})".format(task.flag, matches,
                                                                                       flag_field))

    async def putnoise(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        pass

    async def getnoise(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        pass

    async def havoc(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        pass

logger = logging.getLogger()
handler = logging.StreamHandler(sys.stdout)
handler.setFormatter(ELKFormatter("%(message)s")) #ELK-ready output
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)

app = create_app(WaspChecker()) # mongodb://mongodb:27017
