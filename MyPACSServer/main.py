from server import MyPACSServer
import click


@click.command()
@click.argument('config', default=r'./config.json')
def main(config):
    server = MyPACSServer(config)
    server.run()


if __name__ == '__main__':
    main()
