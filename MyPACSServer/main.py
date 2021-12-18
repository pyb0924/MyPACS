from server import MyPACSServer

if __name__ == '__main__':
    config_root = r'./config.json'
    server = MyPACSServer(config_root)
    server.run()
