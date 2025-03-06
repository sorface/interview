import { RoomInvite } from '../types/room';

export const mapInvitesForAiRoom = (invites: RoomInvite[]): RoomInvite[] =>
  invites
    .filter((invite) => invite.participantType === 'Examinee')
    .map((invite) => ({ ...invite, participantType: 'Viewer' }));
