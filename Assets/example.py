import pygame
import sys
import os

# Initialize Pygame
pygame.init()

# Settings
TILE_SIZE = 16
SCALE = 4  # Scale up for better visibility
SCREEN_WIDTH = 1200
SCREEN_HEIGHT = 800
FPS = 60

# Colors
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
DARK_GRAY = (40, 40, 40)
GRAY = (80, 80, 80)
LIGHT_GRAY = (160, 160, 160)
BLUE = (70, 130, 220)
GREEN = (80, 200, 120)

# Create window
screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
pygame.display.set_caption("Character Animation Tester")
clock = pygame.time.Clock()

# Fonts
title_font = pygame.font.Font(None, 36)
font = pygame.font.Font(None, 24)
small_font = pygame.font.Font(None, 20)

# === CHARACTER CLASS ===
class Character:
    def __init__(self, name, asset_folder):
        self.name = name
        self.asset_folder = asset_folder
        
        # Animation states
        self.animations = {}
        self.current_animation = "idle_anim"
        self.current_direction = "down"
        self.current_frame = 0
        self.frame_counter = 0
        self.animation_speed = 8  # Frames to wait before changing animation frame
        
        # Position
        self.x = SCREEN_WIDTH // 2
        self.y = SCREEN_HEIGHT // 2
        
        # Load all animations
        self.load_animations()
    
    def load_animations(self):
        """Load all animation sprite sheets for this character"""
        animations_info = {
            'idle': {'file': f'{self.name}_idle_16x16.png', 'frames': 2, 'has_hat': True, 'has_directions': True},
            'idle_anim': {'file': f'{self.name}_idle_anim_16x16.png', 'frames': 6, 'has_hat': True, 'has_directions': True},
            'phone': {'file': f'{self.name}_phone_16x16.png', 'frames': 8, 'has_hat': True, 'has_directions': False},
            'run': {'file': f'{self.name}_run_16x16.png', 'frames': 6, 'has_hat': True, 'has_directions': True}
        }
        
        for anim_name, info in animations_info.items():
            filepath = os.path.join(self.asset_folder, info['file'])
            try:
                sheet = pygame.image.load(filepath).convert_alpha()
                self.animations[anim_name] = {
                    'sheet': sheet,
                    'frames': info['frames'],
                    'has_hat': info['has_hat'],
                    'has_directions': info['has_directions']
                }
                print(f"✓ Loaded {anim_name} for {self.name}")
            except Exception as e:
                print(f"✗ Failed to load {filepath}: {e}")
    
    def get_sprite(self, animation, direction, frame, part='body'):
        """Extract a sprite from the animation sheet
        
        The sprite sheets are structured as:
        - Row 0 (y=0): All hats for all directions (right, up, left, down)
        - Row 1 (y=16): All bodies for all directions (right, up, left, down)
        
        For phone animation (no directions):
        - Row 0 (y=0): All hat frames
        - Row 1 (y=16): All body frames
        
        Args:
            animation: 'idle', 'idle_anim', 'phone', 'run'
            direction: 'right', 'up', 'left', 'down'
            frame: frame number (0 to frames-1)
            part: 'hat' or 'body'
        """
        if animation not in self.animations:
            return None
        
        anim_data = self.animations[animation]
        sheet = anim_data['sheet']
        frames_per_direction = anim_data['frames']
        has_directions = anim_data['has_directions']
        
        # Calculate X position
        if has_directions:
            # Direction column offsets (in frames)
            direction_offsets = {
                'right': 0,
                'up': frames_per_direction,
                'left': frames_per_direction * 2,
                'down': frames_per_direction * 3
            }
            
            if direction not in direction_offsets:
                return None
            
            x = (direction_offsets[direction] + frame) * TILE_SIZE
        else:
            # No directions, just frames going left to right
            x = frame * TILE_SIZE
        
        # Calculate Y position: row 0 for hats, row 1 for bodies
        if part == 'hat':
            y = 0
        else:  # body
            y = TILE_SIZE
        
        # Extract sprite
        surface = pygame.Surface((TILE_SIZE, TILE_SIZE), pygame.SRCALPHA)
        surface.blit(sheet, (0, 0), (x, y, TILE_SIZE, TILE_SIZE))
        
        # Scale up
        return pygame.transform.scale(surface, (TILE_SIZE * SCALE, TILE_SIZE * SCALE))
    
    def get_current_sprite(self, part='body'):
        """Get the current sprite based on state"""
        return self.get_sprite(
            self.current_animation,
            self.current_direction,
            self.current_frame,
            part
        )
    
    def update(self):
        """Update animation frame"""
        self.frame_counter += 1
        if self.frame_counter >= self.animation_speed:
            anim_data = self.animations.get(self.current_animation)
            if anim_data:
                self.current_frame = (self.current_frame + 1) % anim_data['frames']
            self.frame_counter = 0
    
    def draw(self, surface):
        """Draw the character - hat stacked on top of body"""
        # Draw body
        body = self.get_current_sprite('body')
        if body:
            surface.blit(body, (self.x - TILE_SIZE * SCALE // 2, self.y - TILE_SIZE * SCALE // 2 + TILE_SIZE * SCALE // 2))
        
        # Draw hat ABOVE body
        hat = self.get_current_sprite('hat')
        if hat:
            surface.blit(hat, (self.x - TILE_SIZE * SCALE // 2, self.y - TILE_SIZE * SCALE // 2 - TILE_SIZE * SCALE // 2))
    
    def set_animation(self, animation):
        """Change animation"""
        if animation in self.animations and animation != self.current_animation:
            self.current_animation = animation
            self.current_frame = 0
            self.frame_counter = 0
    
    def set_direction(self, direction):
        """Change direction"""
        if direction in ['right', 'up', 'left', 'down']:
            self.current_direction = direction

# === UI COMPONENTS ===
class Button:
    def __init__(self, x, y, width, height, text, color=BLUE):
        self.rect = pygame.Rect(x, y, width, height)
        self.text = text
        self.color = color
        self.hover = False
        self.active = False
    
    def draw(self, surface):
        color = GREEN if self.active else (self.color if not self.hover else (100, 160, 255))
        pygame.draw.rect(surface, color, self.rect, border_radius=4)
        pygame.draw.rect(surface, WHITE, self.rect, 2, border_radius=4)
        
        text_surf = font.render(self.text, True, WHITE)
        text_rect = text_surf.get_rect(center=self.rect.center)
        surface.blit(text_surf, text_rect)
    
    def handle_event(self, event):
        if event.type == pygame.MOUSEMOTION:
            self.hover = self.rect.collidepoint(event.pos)
        elif event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
            if self.rect.collidepoint(event.pos):
                return True
        return False

# === MAIN ===
ASSET_FOLDER = "./assets/Characters_free/"

# Available characters
character_names = ['Adam', 'Alex', 'Amelia', 'Bob']
current_character_index = 0

# Load first character
character = Character(character_names[current_character_index], ASSET_FOLDER)

# Create UI buttons
button_y = 50
button_spacing = 110

# Character selection
btn_prev_char = Button(20, 20, 100, 40, "< Prev")
btn_next_char = Button(130, 20, 100, 40, "Next >")

# Animation buttons
animation_buttons = {
    'idle': Button(20, button_y, 100, 40, "Idle"),
    'idle_anim': Button(20, button_y + button_spacing, 100, 40, "Idle Anim"),
    'phone': Button(20, button_y + button_spacing * 2, 100, 40, "Phone"),
    'run': Button(20, button_y + button_spacing * 3, 100, 40, "Run")
}

# Direction buttons
direction_buttons = {
    'right': Button(SCREEN_WIDTH - 230, SCREEN_HEIGHT - 150, 100, 40, "Right"),
    'up': Button(SCREEN_WIDTH - 230, SCREEN_HEIGHT - 200, 100, 40, "Up"),
    'left': Button(SCREEN_WIDTH - 230, SCREEN_HEIGHT - 100, 100, 40, "Left"),
    'down': Button(SCREEN_WIDTH - 230, SCREEN_HEIGHT - 50, 100, 40, "Down")
}

# Speed control
btn_slower = Button(SCREEN_WIDTH - 120, SCREEN_HEIGHT - 150, 100, 40, "Slower")
btn_faster = Button(SCREEN_WIDTH - 120, SCREEN_HEIGHT - 100, 100, 40, "Faster")

# Update active button states
def update_button_states():
    for anim_name, btn in animation_buttons.items():
        btn.active = (character.current_animation == anim_name)
    for dir_name, btn in direction_buttons.items():
        btn.active = (character.current_direction == dir_name)

update_button_states()

# Main loop
running = True
while running:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False
        elif event.type == pygame.KEYDOWN:
            if event.key == pygame.K_ESCAPE:
                running = False
            # Arrow keys for direction
            elif event.key == pygame.K_RIGHT:
                character.set_direction('right')
            elif event.key == pygame.K_LEFT:
                character.set_direction('left')
            elif event.key == pygame.K_UP:
                character.set_direction('up')
            elif event.key == pygame.K_DOWN:
                character.set_direction('down')
            # Number keys for animations
            elif event.key == pygame.K_1:
                character.set_animation('idle')
            elif event.key == pygame.K_2:
                character.set_animation('idle_anim')
            elif event.key == pygame.K_3:
                character.set_animation('phone')
            elif event.key == pygame.K_4:
                character.set_animation('run')
            
            update_button_states()
        
        # Character selection
        if btn_prev_char.handle_event(event):
            current_character_index = (current_character_index - 1) % len(character_names)
            character = Character(character_names[current_character_index], ASSET_FOLDER)
            update_button_states()
        
        if btn_next_char.handle_event(event):
            current_character_index = (current_character_index + 1) % len(character_names)
            character = Character(character_names[current_character_index], ASSET_FOLDER)
            update_button_states()
        
        # Animation buttons
        for anim_name, btn in animation_buttons.items():
            if btn.handle_event(event):
                character.set_animation(anim_name)
                update_button_states()
        
        # Direction buttons
        for dir_name, btn in direction_buttons.items():
            if btn.handle_event(event):
                character.set_direction(dir_name)
                update_button_states()
        
        # Speed control
        if btn_slower.handle_event(event):
            character.animation_speed = min(30, character.animation_speed + 2)
        if btn_faster.handle_event(event):
            character.animation_speed = max(1, character.animation_speed - 2)
    
    # Update
    character.update()
    
    # Draw
    screen.fill(DARK_GRAY)
    
    # Draw character
    character.draw(screen)
    
    # Draw UI panel
    panel_rect = pygame.Rect(0, 0, 250, SCREEN_HEIGHT)
    pygame.draw.rect(screen, GRAY, panel_rect)
    
    # Title
    title = title_font.render("Animation Tester", True, WHITE)
    screen.blit(title, (250, 20))
    
    # Character name
    char_name = font.render(f"Character: {character.name}", True, WHITE)
    screen.blit(char_name, (250, 60))
    
    # Current state
    state_text = small_font.render(f"Animation: {character.current_animation}", True, LIGHT_GRAY)
    screen.blit(state_text, (250, 90))
    
    direction_text = small_font.render(f"Direction: {character.current_direction}", True, LIGHT_GRAY)
    screen.blit(direction_text, (250, 115))
    
    frame_text = small_font.render(f"Frame: {character.current_frame + 1}/{character.animations[character.current_animation]['frames']}", True, LIGHT_GRAY)
    screen.blit(frame_text, (250, 140))
    
    speed_text = small_font.render(f"Speed: {character.animation_speed} frames/step", True, LIGHT_GRAY)
    screen.blit(speed_text, (SCREEN_WIDTH - 230, SCREEN_HEIGHT - 180))
    
    # Instructions
    inst_y = SCREEN_HEIGHT - 350
    instructions = [
        "Keyboard Shortcuts:",
        "• Arrow keys - Direction",
        "• 1 - Idle",
        "• 2 - Idle Anim",
        "• 3 - Phone",
        "• 4 - Run",
        "• ESC - Quit"
    ]
    
    for i, inst in enumerate(instructions):
        color = WHITE if i == 0 else LIGHT_GRAY
        text = small_font.render(inst, True, color)
        screen.blit(text, (20, inst_y + i * 25))
    
    # Draw buttons
    btn_prev_char.draw(screen)
    btn_next_char.draw(screen)
    
    for btn in animation_buttons.values():
        btn.draw(screen)
    
    for btn in direction_buttons.values():
        btn.draw(screen)
    
    btn_slower.draw(screen)
    btn_faster.draw(screen)
    
    pygame.display.flip()
    clock.tick(FPS)

pygame.quit()
sys.exit()