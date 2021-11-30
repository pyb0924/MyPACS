from myPACS_server import MyPACSServer

if __name__ == '__main__':
    server = MyPACSServer('./config.json')
    server.run()
