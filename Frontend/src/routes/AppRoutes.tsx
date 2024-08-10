import React, { FunctionComponent } from 'react';
import { Routes, Route, useLocation, matchPath } from 'react-router-dom';
import { inviteParamName, pathnames } from '../constants';
import { Home } from '../pages/Home/Home';
import { Rooms, RoomsPageMode } from '../pages/Rooms/Rooms';
import { Questions } from '../pages/Questions/Questions';
import { QuestionCreate } from '../pages/QuestionCreate/QuestionCreate';
import { NotFound } from '../pages/NotFound/NotFound';
import { Room } from '../pages/Room/Room';
import { Session } from '../pages/Session/Session';
import { RoomParticipants } from '../pages/RoomParticipants/RoomParticipants';
import { ProtectedRoute } from './ProtectedRoute';
import { User } from '../types/user';
import { Terms } from '../pages/Terms/Terms';
import { RoomAnayticsSummary } from '../pages/RoomAnayticsSummary/RoomAnayticsSummary';
import { NavMenu } from '../components/NavMenu/NavMenu';
import { Categories } from '../pages/Categories/Categories';
import { checkAdmin } from '../utils/checkAdmin';
import { CategoriesCreate } from '../pages/CategoriesCreate/CategoriesCreate';
import { Gap } from '../components/Gap/Gap';
import { RoomReview } from '../pages/RoomReview/RoomReview';
import { RoomAnaytics } from '../pages/RoomAnaytics/RoomAnaytics';

interface AppRoutesProps {
  user: User | null;
}

export const AppRoutes: FunctionComponent<AppRoutesProps> = ({
  user,
}) => {
  const admin = checkAdmin(user);
  const location = useLocation();
  const fullScreenPage = matchPath(
    { path: pathnames.room.replace(`/:${inviteParamName}?`, ''), end: false, },
    location.pathname,
  );
  const authenticated = !!user;

  return (
    <>
      {!fullScreenPage && (
        <NavMenu admin={admin} />
      )}
      <div className={`App ${fullScreenPage ? 'full-screen-page' : ''}`}>
        <div className="App-content">
          <Routes>
            <Route path={pathnames.home} element={<Home />} />
            <Route path={pathnames.terms} element={<Terms />} />
            <Route path={pathnames.roomsParticipants}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomParticipants />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.roomReview}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomReview />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.roomAnalyticsSummary}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomAnayticsSummary />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.roomAnalytics}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomAnaytics />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.room}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Room />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.highlightRooms}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Rooms mode={RoomsPageMode.Home} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.currentRooms}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Rooms mode={RoomsPageMode.Current} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.closedRooms}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Rooms mode={RoomsPageMode.Closed} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.questionsCreate}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionCreate edit={false} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.questionsEdit}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionCreate edit={true} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.questions}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Questions />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.session}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Session />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.categoriesCreate}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <CategoriesCreate edit={false} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.categoriesEdit}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <CategoriesCreate edit={true} />
                </ProtectedRoute>
              }
            />
            <Route path={pathnames.categories}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Categories />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<NotFound />} />
          </Routes>
          {!fullScreenPage && (
            <Gap sizeRem={0.5} />
          )}
        </div>
      </div>
    </>
  );
};
