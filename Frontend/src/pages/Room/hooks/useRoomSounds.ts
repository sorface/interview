import { useCallback, useMemo } from 'react';

const enum Sound {
  JoinRoom = 'join_room',
  ChatMessage = 'chat_message',
}

const prepareAudio = (soundName: Sound, volume: number) => {
  const audio = new Audio(`/audios/fx/${soundName}.mp3`);
  audio.volume = volume;
  return audio;
};

export const useRoomSounds = () => {
  const audios = useMemo(() => ({
    [Sound.JoinRoom]: prepareAudio(Sound.JoinRoom, 0.3),
    [Sound.ChatMessage]: prepareAudio(Sound.JoinRoom, 0.3),
  }), []);

  const playJoinRoomSound = useCallback(() => {
    audios[Sound.JoinRoom].play();
  }, [audios]);

  const playChatMessageSound = useCallback(() => {
    audios[Sound.ChatMessage].play();
  }, [audios]);

  return {
    playJoinRoomSound,
    playChatMessageSound,
  };
};
