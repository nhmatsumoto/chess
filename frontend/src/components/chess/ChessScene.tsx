import { useEffect, useRef, useState } from 'react';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { useGameStore } from '../../store/gameStore';

export const ChessScene = () => {
  const containerRef = useRef<HTMLDivElement>(null);
  const { board, legalMoves, makeMove, fetchLegalMoves, turn } = useGameStore();
  const [selectedPos, setSelectedPos] = useState<string | null>(null);
  
  const sceneRef = useRef<THREE.Scene | null>(null);
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);
  const squaresRef = useRef<THREE.Mesh[]>([]);
  const piecesGroupRef = useRef<THREE.Group | null>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    const width = containerRef.current.clientWidth;
    const height = containerRef.current.clientHeight;

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x0f172a);
    sceneRef.current = scene;

    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    camera.position.set(0, 8, 10);
    cameraRef.current = camera;

    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(width, height);
    renderer.shadowMap.enabled = true;
    containerRef.current.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;

    const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
    scene.add(ambientLight);
    const dirLight = new THREE.DirectionalLight(0xffffff, 0.8);
    dirLight.position.set(5, 10, 5);
    dirLight.castShadow = true;
    scene.add(dirLight);

    const boardGroup = new THREE.Group();
    const squareGeo = new THREE.BoxGeometry(1, 0.1, 1);
    
    // Clear refs on re-init
    squaresRef.current = [];

    for (let f = 0; f < 8; f++) {
      for (let r = 0; r < 8; r++) {
        const isWhite = (f + r) % 2 !== 0;
        const posStr = `${String.fromCharCode(97 + f)}${r + 1}`;
        const whiteMat = new THREE.MeshPhongMaterial({ color: 0xe2e8f0 });
        const blackMat = new THREE.MeshPhongMaterial({ color: 0x334155 });
        const square = new THREE.Mesh(squareGeo, isWhite ? whiteMat : blackMat);
        square.userData = { pos: posStr, isWhite };
        square.position.set(f - 3.5, 0, r - 3.5);
        square.receiveShadow = true;
        boardGroup.add(square);
        squaresRef.current.push(square);
      }
    }
    scene.add(boardGroup);

    const handleWindowResize = () => {
      if (!containerRef.current || !cameraRef.current) return;
      const w = containerRef.current.clientWidth;
      const h = containerRef.current.clientHeight;
      cameraRef.current.aspect = w / h;
      cameraRef.current.updateProjectionMatrix();
      renderer.setSize(w, h);
    };

    window.addEventListener('resize', handleWindowResize);

    const animate = () => {
      requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    };
    animate();

    return () => {
      window.removeEventListener('resize', handleWindowResize);
      renderer.dispose();
      if (containerRef.current && renderer.domElement.parentElement) {
        containerRef.current.removeChild(renderer.domElement);
      }
    };
  }, []);

  // Update squares based on selection and legal moves
  useEffect(() => {
    const highlightMat = new THREE.MeshPhongMaterial({ color: 0x60a5fa, emissive: 0x2563eb });
    const legalMat = new THREE.MeshPhongMaterial({ color: 0x4ade80, emissive: 0x16a34a });
    const whiteMatColors = { color: 0xe2e8f0 };
    const blackMatColors = { color: 0x334155 };

    squaresRef.current.forEach(square => {
      const pos = square.userData.pos;
      if (pos === selectedPos) {
        square.material = highlightMat;
      } else if (legalMoves.includes(pos)) {
        square.material = legalMat;
      } else {
        const isWhite = square.userData.isWhite;
        (square.material as THREE.MeshPhongMaterial).color.set(isWhite ? 0xe2e8f0 : 0x334155);
        (square.material as THREE.MeshPhongMaterial).emissive.set(0x000000);
      }
    });
  }, [selectedPos, legalMoves]);

  // Render pieces
  useEffect(() => {
    if (!sceneRef.current) return;
    if (piecesGroupRef.current) sceneRef.current.remove(piecesGroupRef.current);

    const group = new THREE.Group();
    board.forEach(p => {
      const f = p.pos[0].charCodeAt(0) - 97;
      const r = parseInt(p.pos[1]) - 1;
      const mesh = new THREE.Mesh(
        p.type === 'Pawn' ? new THREE.CylinderGeometry(0.3, 0.4, 0.8) : new THREE.BoxGeometry(0.5, 1.2, 0.5),
        new THREE.MeshPhongMaterial({ color: p.color === 'White' ? 0xffffff : 0x1e293b })
      );
      mesh.position.set(f - 3.5, 0.5, r - 3.5);
      mesh.castShadow = true;
      group.add(mesh);
    });
    sceneRef.current.add(group);
    piecesGroupRef.current = group;
  }, [board]);

  const onBoardClick = (event: React.MouseEvent) => {
    if (!containerRef.current || !cameraRef.current) return;
    const rect = containerRef.current.getBoundingClientRect();
    const mouse = new THREE.Vector2(
      ((event.clientX - rect.left) / rect.width) * 2 - 1,
      -((event.clientY - rect.top) / rect.height) * 2 + 1
    );

    const raycaster = new THREE.Raycaster();
    raycaster.setFromCamera(mouse, cameraRef.current);
    const intersects = raycaster.intersectObjects(squaresRef.current, false);

    if (intersects.length > 0) {
      const square = intersects[0].object as THREE.Mesh;
      const pos = square.userData.pos;

      if (selectedPos) {
        if (legalMoves.includes(pos)) {
          makeMove(selectedPos, pos);
          setSelectedPos(null);
        } else {
          // Check if clicked square has a piece of current turn color
          const piece = board.find(p => p.pos === pos && p.color === turn);
          if (piece) {
            setSelectedPos(pos);
            fetchLegalMoves(pos);
          } else {
            setSelectedPos(null);
          }
        }
      } else {
        const piece = board.find(p => p.pos === pos && p.color === turn);
        if (piece) {
          setSelectedPos(pos);
          fetchLegalMoves(pos);
        }
      }
    }
  };

  return <div ref={containerRef} className="w-full h-full cursor-pointer" onClick={onBoardClick} />;
};
