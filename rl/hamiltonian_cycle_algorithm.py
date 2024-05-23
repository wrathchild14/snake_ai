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


def get_direction(node1, node2):
    y1, x1 = node1
    y2, x2 = node2
    if y1 < y2:
        return 3
    elif y2 < y1:
        return 1
    elif x1 < x2:
        return 2
    elif x2 < x1:
        return 0


def prims_grid_algorithm(rows, columns):
    """
    generates minimum spanning tree for a grid of a size (rows, columns)
    """
    minimum_spanning_tree = [[[0, 0, 0, 0] for _ in range(columns)] for _ in range(rows)]
    minimum_spanning_edges = []
    queue = [(i, j) for i in range(rows) for j in range(columns)]
    first_node = choice(queue)
    queue.remove(first_node)
    visited = [first_node]

    while len(queue) > 0:
        current_valid_edges = generate_valid_prims_edges(visited, rows, columns)
        current_edge = choice(current_valid_edges)
        node, new_node = current_edge
        visited.append(new_node)
        queue.remove(new_node)

        minimum_spanning_edges.append(current_edge)
        directions = get_direction(node, new_node)
        minimum_spanning_tree[node[0]][node[1]][directions] = 1

    return minimum_spanning_tree


def hamiltonian_grid_cycle(rows, columns):
    """
    generates a hamiltonian cycle for a grid of a size (rows, columns)
    """
    min_spanning_tree = prims_grid_algorithm(rows, columns)
    print(min_spanning_tree)


hamiltonian_grid_cycle(2, 3)
