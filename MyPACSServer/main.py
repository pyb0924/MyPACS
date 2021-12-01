from server import MyPACSServer
from utils import config_root

if __name__ == '__main__':
    server = MyPACSServer(config_root)
    server.run()
