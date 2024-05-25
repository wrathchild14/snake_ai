from random import choice

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


def get_direction_too_adjacent_node(node1, node2):
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


def prims_algorithm_for_grid(rows, columns):
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

        direction1to2 = get_direction_too_adjacent_node(node1, node2)
        minimum_spanning_tree[node1[0]][node1[1]].append(direction1to2)
        direction2to1 = get_direction_too_adjacent_node(node2, node1)
        minimum_spanning_tree[node2[0]][node2[1]].append(direction2to1)

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
    """
    traverse the grid and return the coordinates of the path
    """
    current_direction = RIGHT
    current_node = [0, 0]
    mst_path = []
    directions = []
    for _ in range(len(grid) * len(grid[0]) * 2 - 2):
        mst_path.append(current_node)
        current_node_connections = grid[current_node[0]][current_node[1]]
        current_direction = turn_left(current_direction)
        if current_direction in current_node_connections:
            current_node = get_next_node(current_node, current_direction)
            directions.append(current_direction)
        else:
            for i in range(3):
                current_direction = get_next_direction(current_direction)
                if current_direction in current_node_connections:
                    current_node = get_next_node(current_node, current_direction)
                    directions.append(current_direction)
                    break
    return mst_path, directions


def get_path_between_nodes(current_node, direction1, direction2):
    y, x = current_node
    path = []
    # First LEFT
    if direction1 == LEFT and direction2 == LEFT:
        path.extend([[y, x - 1], [y, x - 2]])
    elif direction1 == LEFT and direction2 == UP:
        path.extend([[y, x - 1], [y - 1, x - 1], [y - 2, x - 1]])
    elif direction1 == LEFT and direction2 == RIGHT:
        path.extend([[y, x - 1], [y - 1, x - 1], [y - 1, x], [y - 1, x + 1]])
    elif direction1 == LEFT and direction2 == DOWN:
        path.extend([[y + 1, x]])
    # First UP
    elif direction1 == UP and direction2 == LEFT:
        path.extend([[y, x - 1]])
    elif direction1 == UP and direction2 == UP:
        path.extend([[y - 1, x], [y - 2, x]])
    elif direction1 == UP and direction2 == RIGHT:
        path.extend([[y - 1, x], [y - 1, x + 1], [y - 1, x + 2]])
    elif direction1 == UP and direction2 == DOWN:
        path.extend([[y - 1, x], [y - 1, x + 1], [y, x + 1], [y + 1, x + 1]])
    # First RIGHT
    elif direction1 == RIGHT and direction2 == LEFT:
        path.extend([[y, x + 1], [y + 1, x + 1], [y + 1, x], [y + 1, x - 1]])
    elif direction1 == RIGHT and direction2 == UP:
        path.extend([[y - 1, x]])
    elif direction1 == RIGHT and direction2 == RIGHT:
        path.extend([[y, x + 1], [y, x + 2]])
    elif direction1 == RIGHT and direction2 == DOWN:
        path.extend([[y, x + 1], [y + 1, x + 1], [y + 2, x + 1]])
    # First DOWN
    elif direction1 == DOWN and direction2 == LEFT:
        path.extend([[y + 1, x], [y + 1, x - 1], [y + 1, x - 2]])
    elif direction1 == DOWN and direction2 == UP:
        path.extend([[y + 1, x], [y + 1, x - 1], [y, x - 1], [y - 1, x - 1]])
    elif direction1 == DOWN and direction2 == RIGHT:
        path.extend([[y, x + 1]])
    elif direction1 == DOWN and direction2 == DOWN:
        path.extend([[y + 1, x], [y + 2, x]])

    current_node = path[-1]
    return path, current_node


def generate_hamiltonian_path(directions):
    current_direction = RIGHT
    current_node = [0, 0]
    hamiltonian_path = [current_node]
    for direction in directions:
        paths, current_node = get_path_between_nodes(current_node, current_direction, direction)
        current_direction = direction
        hamiltonian_path.extend(paths)
    return hamiltonian_path


def hamiltonian_cycle_for_grid(rows, columns):
    """
    generates a hamiltonian cycle for a grid of a size (rows, columns)
    """
    min_spanning_tree = prims_algorithm_for_grid(rows, columns)
    mst_path, directions = traverse_grid(min_spanning_tree)
    hamiltonian_path = generate_hamiltonian_path(directions)
    return hamiltonian_path


if __name__ == '__main__':
    cycle = hamiltonian_cycle_for_grid(2, 2)
    print(cycle)
