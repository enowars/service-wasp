import requests
import json
import random
import string
from enochecker import BaseChecker, BrokenServiceException, run

session = requests.Session()


class WaspChecker(BaseChecker):

    port = 5000  # default port to send requests to.


    def putflag(self):
        tag = ''.join(random.choice(string.ascii_uppercase + string.digits) for _ in range(10))
        self.team_db[self.flag] = tag

        self.debug("Putting Flag...")
        # / because why not
        self.http_get("/")
        if self.flag_idx % 2 == 0:
            attack = {
                "date": self.flag,
                "location": "Berlin",
                "description": "tasty bee hive! #{}".format(tag),
                "password": self.flag
            }
        else:
            attack = {
                "date": "whenever",
                "location": self.flag,
                "description": "tasty bee hive! #{}".format(tag),
                "password": self.flag
            }

        # /AddAttack
        self.http_post("/api/AddAttack", data=attack)
        self.debug("Flag {} up with tag: {}.".format(self.flag, tag))

    def getflag(self):
        self.http_get("/")

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

    def putnoise(self):
        pass

    def getnoise(self):
        pass

    def havoc(self):
        pass


app = WaspChecker.service
if __name__ == "__main__":
    run(WaspChecker)
