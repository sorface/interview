import React, { FunctionComponent, useEffect } from 'react';
import {
  ReactFlow,
  ColorMode,
  Node,
  Edge,
  useNodesState,
  useEdgesState,
  Position,
} from '@xyflow/react';
import dagre from '@dagrejs/dagre';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';
import { TreeNode } from '../../types/tree';

import '@xyflow/react/dist/style.css';
import './TreeViewer.css';

const dagreGraph = new dagre.graphlib.Graph().setDefaultEdgeLabel(() => ({}));

const nodeWidth = 172;
const nodeHeight = 36;

const getLayoutedElements = (
  nodes: Node[],
  edges: Edge[],
  direction = 'TB',
) => {
  const isHorizontal = direction === 'LR';
  dagreGraph.setGraph({ rankdir: direction });

  nodes.forEach((node) => {
    dagreGraph.setNode(node.id, { width: nodeWidth, height: nodeHeight });
  });

  edges.forEach((edge) => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  dagre.layout(dagreGraph);

  const newNodes = nodes.map((node) => {
    const nodeWithPosition = dagreGraph.node(node.id);
    const newNode = {
      ...node,
      targetPosition: isHorizontal ? Position.Left : Position.Top,
      sourcePosition: isHorizontal ? Position.Right : Position.Bottom,
      // We are shifting the dagre node position (anchor=center center) to the top left
      // so it matches the React Flow node anchor point (top left).
      position: {
        x: nodeWithPosition.x - nodeWidth / 2,
        y: nodeWithPosition.y - nodeHeight / 2,
      },
    };

    return newNode;
  });

  return { nodes: newNodes, edges };
};

const defaultPosition = { x: 0, y: 0 };

const getNodes = (tree: TreeNode[]): Node[] => {
  return tree.map((node) => ({
    id: node.id,
    position: defaultPosition,
    data: { label: node.question?.value },
  }));
};

const getEdges = (tree: TreeNode[]): Edge[] => {
  const edges: Edge[] = [];
  tree.forEach((node) => {
    if (!node.parentQuestionSubjectTreeId) {
      return;
    }
    edges.push({
      id: `${node.parentQuestionSubjectTreeId}${node.id}`,
      source: node.parentQuestionSubjectTreeId,
      target: node.id,
    });
  });
  return edges;
};

interface TreeViewerProps {
  tree: TreeNode[];
}

export const TreeViewer: FunctionComponent<TreeViewerProps> = ({ tree }) => {
  const reactFlowColorMode: ColorMode = useThemeClassName({
    [Theme.Dark]: 'dark',
    [Theme.Light]: 'light',
  });
  const [nodes, setNodes] = useNodesState<Node>([]);
  const [edges, setEdges] = useEdgesState<Edge>([]);

  useEffect(() => {
    const nodes = getNodes(tree);
    const edges = getEdges(tree);
    const { nodes: layoutedNodes, edges: layoutedEdges } = getLayoutedElements(
      nodes,
      edges,
      'TB',
    );
    setNodes(layoutedNodes);
    setEdges(layoutedEdges);
  }, [tree]);

  return (
    <ReactFlow
      fitView
      nodesConnectable={false}
      nodesFocusable={false}
      nodesDraggable={false}
      colorMode={reactFlowColorMode}
      nodes={nodes}
      edges={edges}
      // onNodesChange={onNodesChange}
      // onEdgesChange={onEdgesChange}
      // onConnect={onConnect}
    />
  );
};
