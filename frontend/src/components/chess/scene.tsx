import { component$, useVisibleTask$, useSignal, $ } from '@builder.io/qwik';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';

interface ChessSceneProps {
  board: any[];
  onMove$?: (from: string, to: string) => void;
}

export const ChessScene = component$((props: ChessSceneProps) => {
  const containerRef = useSignal<Element>();
  const selectedPos = useSignal<string | null>(null);

  useVisibleTask$(({ cleanup }) => {
    if (!containerRef.value) return;

    const width = containerRef.value.clientWidth;
    const height = containerRef.value.clientHeight;

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x0f172a);
    
    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    camera.position.set(0, 8, 10);
    
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(width, height);
    renderer.shadowMap.enabled = true;
    containerRef.value.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;

    // Raycaster for selection
    const raycaster = new THREE.Raycaster();
    const mouse = new THREE.Vector2();

    // BOARD and LIGHTING (omitting for brevity in thought, but keeping in code)
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
    scene.add(ambientLight);
    const dirLight = new THREE.DirectionalLight(0xffffff, 0.8);
    dirLight.position.set(5, 10, 5);
    dirLight.castShadow = true;
    scene.add(dirLight);

    const boardGroup = new THREE.Group();
    const squares: THREE.Mesh[] = [];
    const squareGeo = new THREE.BoxGeometry(1, 0.1, 1);
    const whiteMat = new THREE.MeshPhongMaterial({ color: 0xe2e8f0 });
    const blackMat = new THREE.MeshPhongMaterial({ color: 0x334155 });
    const highlightMat = new THREE.MeshPhongMaterial({ color: 0x60a5fa, emissive: 0x2563eb });

    for (let f = 0; f < 8; f++) {
      for (let r = 0; r < 8; r++) {
        const isWhite = (f + r) % 2 !== 0;
        const square = new THREE.Mesh(squareGeo, isWhite ? whiteMat : blackMat);
        const posStr = `${String.fromCharCode(97 + f)}${r + 1}`;
        square.name = `square_${posStr}`;
        square.userData = { pos: posStr };
        square.position.set(f - 3.5, 0, r - 3.5);
        square.receiveShadow = true;
        boardGroup.add(square);
        squares.push(square);
      }
    }
    scene.add(boardGroup);

    const renderPieces = (boardData: any[]) => {
      const existing = scene.getObjectByName('piecesGroup');
      if (existing) scene.remove(existing);
      const piecesGroup = new THREE.Group();
      piecesGroup.name = 'piecesGroup';
      boardData.forEach(p => {
        const f = p.pos[0].charCodeAt(0) - 97;
        const r = parseInt(p.pos[1]) - 1;
        const mesh = new THREE.Mesh(
          p.type === 'Pawn' ? new THREE.CylinderGeometry(0.3, 0.4, 0.8) : new THREE.BoxGeometry(0.5, 1.2, 0.5),
          new THREE.MeshPhongMaterial({ color: p.color === 'White' ? 0xffffff : 0x1e293b })
        );
        mesh.position.set(f - 3.5, 0.5, r - 3.5);
        mesh.userData = { pos: p.pos };
        mesh.castShadow = true;
        piecesGroup.add(mesh);
      });
      scene.add(piecesGroup);
    };

    renderPieces(props.board);

    const handleClick = (event: MouseEvent) => {
      const rect = containerRef.value!.getBoundingClientRect();
      mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
      mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

      raycaster.setFromCamera(mouse, camera);
      const intersects = raycaster.intersectObjects(squares, false);

      if (intersects.length > 0) {
        const square = intersects[0].object as THREE.Mesh;
        const pos = square.userData.pos;

        if (selectedPos.value) {
          if (props.onMove$) props.onMove$(selectedPos.value, pos);
          selectedPos.value = null;
          // Reset highlights
          squares.forEach(s => s.material = (s.userData.pos[0].charCodeAt(0) - 97 + parseInt(s.userData.pos[1]) - 1) % 2 !== 0 ? whiteMat : blackMat);
        } else {
          selectedPos.value = pos;
          square.material = highlightMat;
        }
      }
    };

    containerRef.value.addEventListener('click', (e) => handleClick(e as MouseEvent));

    let animationId: number;
    const animate = () => {
      animationId = requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    };
    animate();

    cleanup(() => {
      cancelAnimationFrame(animationId);
      renderer.dispose();
    });
  });

  return <div ref={containerRef} class="w-full h-full" />;
});
