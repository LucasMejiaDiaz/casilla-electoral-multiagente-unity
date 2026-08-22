from flask import Flask, jsonify
from flasgger import Swagger
from mesa import Agent, Model

class WealthAgent(Agent):
        """Agent that exchanges one unit of wealth with another agent."""

        def __init__(self, model: Model, x: int, y: int) -> None:
                super().__init__(model)
                self.x = x
                self.y = y
                self.wealth = 1
                self.state = "active"

        def step(self) -> None:
                agents = list(self.model.agents)
                other_agents = [agent for agent in agents if agent is not self]

                self.x = self.model.random.randrange(self.model.width)
                self.y = self.model.random.randrange(self.model.height)

                if other_agents and self.wealth > 0:
                        receiver = self.model.random.choice(other_agents)
                        self.wealth -= 1
                        receiver.wealth += 1
                        self.state = f"gave wealth to agent {receiver.unique_id}"
                else:
                        self.state = "active"


class WealthModel(Model):
        def __init__(self, agent_count: int = 10, width: int = 10, height: int = 10) -> None:
                super().__init__()
                self.width = width
                self.height = height

                for _ in range(agent_count):
                        WealthAgent(
                                self,
                                self.random.randrange(width),
                                self.random.randrange(height),
                        )

        def step(self) -> None:
                self.agents.shuffle(inplace=True).do("step")


app = Flask(__name__)
swagger = Swagger(app)
model = WealthModel()


@app.get("/get_agents")
def get_agents():
        """Get all agents and advance the simulation by one tick.
        ---
        responses:
            200:
                description: Current wealth-agent information.
                schema:
                    type: object
                    properties:
                        step:
                            type: integer
                        agents:
                            type: array
                            items:
                                type: object
                                properties:
                                    id:
                                        type: integer
                                    x:
                                        type: integer
                                    y:
                                        type: integer
                                    wealth:
                                        type: integer
                                    state:
                                        type: string
        """
        model.step()
        agents = [
                {
                        "id": agent.unique_id,
                        "x": agent.x,
                        "y": agent.y,
                        "wealth": agent.wealth,
                        "state": agent.state,
                }
                for agent in model.agents
        ]
        return jsonify({"step": model.steps, "agents": agents})


if __name__ == "__main__":
        app.run(host="127.0.0.1", port=5000, debug=True)
