from random import choice
import numpy as np

# Direction
LEFT = 0
UP = 1
RIGHT = 2
DOWN = 3


def generate_valid_prims_edges(visited, rows, columns):
    current_valid_edges = []
    for node in visited:
        # Left
        if not node[1] - 1 < 0:
            left_node = (node[0], node[1] - 1)
            if left_node not in visited:
                current_valid_edges.append([node, left_node])
        # Right
        if not node[1] + 1 >= columns:
            right_node = (node[0], node[1] + 1)
            if right_node not in visited:
                current_valid_edges.append([node, right_node])
        # Up
        if not node[0] - 1 < 0:
            up_node = (node[0] - 1, node[1])
            if up_node not in visited:
                current_valid_edges.append([node, up_node])
        # Down
        if not node[0] + 1 >= rows:
            down_node = (node[0] + 1, node[1])
            if down_node not in visited:
                current_valid_edges.append([node, down_node])

    return current_valid_edges


def get_direction(node1, node2):
    y1, x1 = node1
    y2, x2 = node2
    if x2 < x1:
        return LEFT
    elif y2 < y1:
        return UP
    elif x1 < x2:
        return RIGHT
    elif y1 < y2:
        return DOWN


def prims_grid_algorithm(rows, columns):
    """
    generates minimum spanning tree for a grid of a size (rows, columns)
    """
    minimum_spanning_tree = [[[] for _ in range(columns)] for _ in range(rows)]
    queue = [(i, j) for i in range(rows) for j in range(columns)]
    first_node = choice(queue)
    queue.remove(first_node)
    visited = [first_node]

    while len(queue) > 0:
        current_valid_edges = generate_valid_prims_edges(visited, rows, columns)
        current_edge = choice(current_valid_edges)
        node1, node2 = current_edge
        visited.append(node2)
        queue.remove(node2)

        direction1 = get_direction(node1, node2)
        minimum_spanning_tree[node1[0]][node1[1]].append(direction1)
        direction2 = get_direction(node2, node1)
        minimum_spanning_tree[node2[0]][node2[1]].append(direction2)

    return minimum_spanning_tree


def get_next_direction(direction):
    return (direction + 1) % 4


def turn_left(direction):
    return (direction - 1) if direction > 0 else 3


def get_next_node(current_node, direction_of_travel):
    if direction_of_travel == LEFT:
        return [current_node[0], current_node[1] - 1]
    elif direction_of_travel == UP:
        return [current_node[0] - 1, current_node[1]]
    elif direction_of_travel == RIGHT:
        return [current_node[0], current_node[1] + 1]
    elif direction_of_travel == DOWN:
        return [current_node[0] + 1, current_node[1]]


def traverse_grid(grid):
    current_direction = RIGHT
    current_node = [0, 0]
    path = []
    for _ in range(len(grid) * len(grid[0]) * 2 - 1):
        path.append(current_node)
        current_node_connections = grid[current_node[0]][current_node[1]]
        current_direction = turn_left(current_direction)
        if current_direction in current_node_connections:
            current_node = get_next_node(current_node, current_direction)
        else:
            for i in range(3):
                current_direction = get_next_direction(current_direction)
                if current_direction in current_node_connections:
                    current_node = get_next_node(current_node, current_direction)
                    break
    return path


def hamiltonian_grid_cycle(rows, columns):
    """
    generates a hamiltonian cycle for a grid of a size (rows, columns)
    """
    min_spanning_tree = prims_grid_algorithm(rows, columns)
    print(min_spanning_tree)
    print(traverse_grid(min_spanning_tree))


hamiltonian_grid_cycle(2, 3)
