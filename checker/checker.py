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
        super(WaspChecker, self).__init__("WASP", 8080)

    async def putflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        tag = ''.join(random.choice(string.ascii_uppercase + string.digits) for _ in range(10))
        await collection.insert_one({ 'flag' : task.flag, 'tag': tag })

        logger.debug("Putting Flag...")
        async with aiohttp.ClientSession() as session:
            # / because why not
            await session.get("http://" + task.address + ":" + str(WaspChecker.port))
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
            await session.post("http://" + task.address + ":" + str(WaspChecker.port) + "/api/AddAttack", data=attack)
            logger.debug("Flag {} up with tag: {}.".format(task.flag, tag))

    async def getflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        return
        await self.test(logger, collection)
        try:
            tag = self.team_db[self.flag]
        except KeyError as ex:
            raise BrokenServiceException("Inconsistent Database: Couldn't get tag for team/flag ({})".format(self.flag))

        r = self.http_get("/api/SearchAttacks", params={"needle": tag})
        self.info("Parsing search result")
        try:
            search_results = json.loads(r.text)
            id = search_results["matches"][0]["id"]
        except Exception as ex:
            raise BrokenServiceException("Invalid JSON Response: {} ({})".format(r.text, ex))

        self.info("Found attack (id={})".format(id))
        self.info("Fetching attack: {}".format({"id": id, "password": self.flag}))

        r = self.http_get("/api/GetAttack", params={"id": id, "password": self.flag}, timeout=5, verify=False)
        self.info("Parsing GetAttack result")
        try:
            attack_results = json.loads(r.text)
        except Exception:
            raise BrokenServiceException("Invalid JSON: {}".format(r.text))

        try:
            flag_field = "attackDate" if self.flag_idx % 2 == 0 else "location"
            if attack_results["attack"][flag_field] != self.flag:
                raise BrokenServiceException(
                    "Incorrect flag in date field (searched for {} in {} - {})".format(self.flag, attack_results,
                                                                                       flag_field))
        except Exception as ex:
            if isinstance(ex, BrokenServiceException):
                raise
            raise BrokenServiceException(
                "Error parsing json: {}. {} (expected: {})".format(attack_results, ex, self.flag))

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
