from random import choice


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


def prims_grid_algorithm(rows, columns):
    """
    generates minimum spanning tree for a grid of a size (rows, columns)
    """
    # minimum_spanning_tree = [[0, 0, 0, 0] for _ in range(rows*columns)]
    minimum_spanning_tree = []
    queue = [(i, j) for i in range(rows) for j in range(columns)]
    visited = [queue.pop(0)]

    while len(queue) > 0:
        current_valid_edges = generate_valid_prims_edges(visited, rows, columns)
        current_edge = choice(current_valid_edges)
        minimum_spanning_tree.append(current_edge)
        visited.append(current_edge[1])
        queue.remove(current_edge[1])

    return minimum_spanning_tree


def hamiltonian_grid_cycle(n, m):
    """
    generates a hamiltonian cycle for a grid of a size (rows, columns)
    """
    pass


print(prims_grid_algorithm(2, 3))
