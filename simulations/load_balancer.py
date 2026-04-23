import asyncio
import random
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

class Server:
    def __init__(self, server_id, capacity):
        self.server_id = server_id
        self.capacity = capacity
        self.current_connections = 0

    @property
    def cpu_load(self):
        return (self.current_connections / self.capacity) * 100

    async def handle_request(self, request_id, processing_time):
        self.current_connections += 1
        load = self.cpu_load

        logger.info(f"[Server {self.server_id}] Handling Request {request_id}. "
                    f"Active Connections: {self.current_connections}/{self.capacity} "
                    f"({load:.1f}% CPU Load)")

        if load >= 90:
            logger.warning(f"!!! ALERT: Server {self.server_id} is approaching 100% CPU load! (Current: {load:.1f}%)")

        try:
            await asyncio.sleep(processing_time)
        finally:
            self.current_connections -= 1
            logger.info(f"[Server {self.server_id}] Finished Request {request_id}. "
                        f"Active Connections: {self.current_connections}")

class LoadBalancer:
    def __init__(self, servers):
        self.servers = servers

    def get_least_connections_server(self):
        # Least Connections algorithm
        return min(self.servers, key=lambda s: s.current_connections)

    async def route_request(self, request_id, processing_time):
        server = self.get_least_connections_server()

        # Check if server has capacity. Although not strictly required by the prompt
        # to block, it's good practice. But the prompt says "approaching 100% CPU load",
        # so let's allow it to hit 100%.

        logger.info(f"[LoadBalancer] Routing Request {request_id} to Server {server.server_id}")
        await server.handle_request(request_id, processing_time)

async def simulate():
    # 1. Define 3 backend servers with a capacity limit
    servers = [
        Server(server_id=1, capacity=10),
        Server(server_id=2, capacity=10),
        Server(server_id=3, capacity=10),
    ]

    lb = LoadBalancer(servers)

    # 2. Generate 50 concurrent 'requests' with random processing times
    num_requests = 50
    tasks = []

    for i in range(num_requests):
        processing_time = random.uniform(0.5, 3.0)
        tasks.append(lb.route_request(request_id=i+1, processing_time=processing_time))
        # Add a tiny delay between spawning requests to see the dynamic distribution
        await asyncio.sleep(0.05)

    logger.info(f"Dispatched all {num_requests} requests.")
    await asyncio.gather(*tasks)
    logger.info("All requests processed.")

if __name__ == "__main__":
    asyncio.run(simulate())
